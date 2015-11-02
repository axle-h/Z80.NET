﻿namespace Axh.Retro.CPU.X80.Memory
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Axh.Retro.CPU.X80.Contracts.Exceptions;
    using Axh.Retro.CPU.X80.Contracts.Memory;

    public class SegmentMmu : IMmu
    {
        private readonly ushort[] readSegmentAddresses;
        private readonly IReadableAddressSegment[] readSegments;

        private readonly ushort[] writeSegmentAddresses;
        private readonly IWriteableAddressSegment[] writeSegments;

        public SegmentMmu(IEnumerable<IAddressSegment> addressSegments)
        {
            var sortedSegments = addressSegments.OrderBy(x => x.Address).ToArray();

            this.readSegments = sortedSegments.OfType<IReadableAddressSegment>().ToArray();
            this.readSegmentAddresses = this.readSegments.Select(x => x.Address).ToArray();

            this.writeSegments = sortedSegments.OfType<IWriteableAddressSegment>().ToArray();
            this.writeSegmentAddresses = this.writeSegments.Select(x => x.Address).ToArray();
            
            CheckSegments(this.readSegments);
            CheckSegments(this.writeSegments);
        }
        
        public byte ReadByte(ushort address)
        {
            ushort segmentAddress;
            var addressSegment = GetAddressSegmentForAddress(this.readSegmentAddresses, this.readSegments, address, out segmentAddress);
            return addressSegment.ReadByte(segmentAddress);
        }

        public ushort ReadWord(ushort address)
        {
            ushort segmentAddress;
            int segmentIndex;
            IReadableAddressSegment segment;
            if (TryGetSegmentIndexForAddress(this.readSegmentAddresses, this.readSegments, address, sizeof(ushort), out segment, out segmentIndex, out segmentAddress))
            {
                return this.readSegments[segmentIndex].ReadWord(segmentAddress);
            }

            // Read one byte from the end of the returned segment index and another from the start of the next
            var lsb = segment.ReadByte(segmentAddress);
            var msb = this.readSegments[(segmentIndex + 1) % this.readSegments.Length].ReadByte(0);
            return BitConverter.ToUInt16(new[] {lsb, msb}, 0);
        }

        public byte[] ReadBytes(ushort address, int length)
        {
            ushort segmentAddress;
            int segmentIndex;
            IReadableAddressSegment segment;
            if (TryGetSegmentIndexForAddress(this.readSegmentAddresses, this.readSegments, address, length, out segment, out segmentIndex, out segmentAddress))
            {
                return this.readSegments[segmentIndex].ReadBytes(segmentAddress, length);
            }

            var bytes = new byte[length];

            // Read from first segment
            var nextSegment = this.readSegments[segmentIndex];
            var segmentLength = nextSegment.Length - segmentAddress;
            var nextBytes = nextSegment.ReadBytes(segmentAddress, segmentLength);
            Array.Copy(nextBytes, 0, bytes, 0, segmentLength);
            var lengthRemaining = length - segmentLength;

            // Read from consecutive segments until all bytes have been read.
            while (lengthRemaining > 0)
            {
                segmentIndex = (segmentIndex + 1) % this.readSegments.Length;
                nextSegment = this.readSegments[segmentIndex];
                segmentLength = Math.Min(lengthRemaining, nextSegment.Length);
                nextBytes = nextSegment.ReadBytes(0, segmentLength);

                Array.Copy(nextBytes, 0, bytes, length - lengthRemaining, segmentLength);
                lengthRemaining -= segmentLength;
            }
            
            return bytes;
        }
        
        public void WriteByte(ushort address, byte value)
        {
            ushort segmentAddress;
            var segment = GetAddressSegmentForAddress(this.writeSegmentAddresses, this.writeSegments, address, out segmentAddress);
            segment.WriteByte(segmentAddress, value);

            if (TriggerWriteEventForMemoryBankType(segment.Type))
            {
                OnAddressWrite(address, 1);
            }
        }
        
        public void WriteWord(ushort address, ushort word)
        {
            ushort segmentAddress;
            int segmentIndex;
            IWriteableAddressSegment segment;
            if (TryGetSegmentIndexForAddress(this.writeSegmentAddresses, this.writeSegments, address, sizeof(ushort), out segment, out segmentIndex, out segmentAddress))
            {
                segment.WriteWord(segmentAddress, word);
                if (TriggerWriteEventForMemoryBankType(segment.Type))
                {
                    OnAddressWrite(address, 2);
                }
                return;
            }

            // Write one byte to the end of the returned segment index and another to the start of the next
            var bytes = BitConverter.GetBytes(word);
            segment.WriteByte(segmentAddress, bytes[0]);
            var nextSegment = this.writeSegments[(segmentIndex + 1) % this.writeSegments.Length];

            if (TriggerWriteEventForMemoryBankType(segment.Type) || TriggerWriteEventForMemoryBankType(nextSegment.Type))
            {
                OnAddressWrite(address, 2);
            }

            nextSegment.WriteByte(0, bytes[1]);
        }

        public void WriteBytes(ushort address, byte[] bytes)
        {
            ushort segmentAddress;
            int segmentIndex;
            IWriteableAddressSegment segment;
            if (TryGetSegmentIndexForAddress(this.writeSegmentAddresses, this.writeSegments, address, bytes.Length, out segment, out segmentIndex, out segmentAddress))
            {
                segment.WriteBytes(segmentAddress, bytes);
                if (TriggerWriteEventForMemoryBankType(segment.Type))
                {
                    OnAddressWrite(address, (ushort)bytes.Length);
                }
                return;
            }
            
            // Write to first segment
            var segmentLength = segment.Length - segmentAddress;
            var nextBytes = new byte[segmentLength];
            Array.Copy(bytes, 0, nextBytes, 0, segmentLength);
            segment.WriteBytes(segmentAddress, nextBytes);
            var nextIndex = segmentLength;
            var lengthRemaining = bytes.Length - segmentLength;

            var triggerWriteEvent = TriggerWriteEventForMemoryBankType(segment.Type);

            // Write to consecutive segments until all bytes have been written.
            while (lengthRemaining > 0)
            {
                segmentIndex = (segmentIndex + 1) % this.writeSegments.Length;
                segment = this.writeSegments[segmentIndex];
                
                segmentLength = Math.Min(lengthRemaining, segment.Length);
                nextBytes = new byte[segmentLength];
                Array.Copy(bytes, nextIndex, nextBytes, 0, segmentLength);
                segment.WriteBytes(0, nextBytes);

                triggerWriteEvent |= TriggerWriteEventForMemoryBankType(segment.Type);

                lengthRemaining -= segmentLength;
                nextIndex += segmentLength;
            }

            if (triggerWriteEvent)
            {
                OnAddressWrite(address, (ushort)bytes.Length);
            }
        }

        public void TransferByte(ushort addressFrom, ushort addressTo)
        {
            var b = this.ReadByte(addressFrom);
            this.WriteByte(addressTo, b);
        }

        public event EventHandler<AddressWriteEventArgs> AddressWrite;
        
        protected void OnAddressWrite(ushort address, ushort length)
        {
            AddressWrite?.Invoke(this, new AddressWriteEventArgs(address, length));
        }

        private static TAddressSegment GetAddressSegmentForAddress<TAddressSegment>(ushort[] segmentAddresses, IList<TAddressSegment> segments, ushort address, out ushort segmentAddress)
            where TAddressSegment : IAddressSegment
        {
            var index = Array.BinarySearch(segmentAddresses, address);

            // If the index is negative, it represents the bitwise 
            // complement of the next larger element in the array. 
            if (index < 0)
            {
                index = ~index - 1;
            }

            var segment = segments[index];

            segmentAddress = (ushort)(address - segment.Address);

            return segment;
        }

        private static bool TryGetSegmentIndexForAddress<TAddressSegment>(ushort[] segmentAddresses, IList<TAddressSegment> segments, ushort address, int length, out TAddressSegment segment, out int segmentIndex, out ushort segmentAddress)
            where TAddressSegment : IAddressSegment
        {
            segmentIndex = Array.BinarySearch(segmentAddresses, address);

            // If the index is negative, it represents the bitwise 
            // complement of the next larger element in the array. 
            if (segmentIndex < 0)
            {
                segmentIndex = ~segmentIndex - 1;
            }

            segment = segments[segmentIndex];

            segmentAddress = (ushort)(address - segment.Address);

            return segmentAddress + length < segment.Length;
        }


        private static void CheckSegments(IEnumerable<IAddressSegment> addressSegments)
        {
            ushort lastAddress = 0x0000;
            foreach (var segment in addressSegments)
            {
                if (segment.Length < 1)
                {
                    throw new PlatformConfigurationException(string.Format("Segment length is less than 1 at 0x{0:x4}", segment.Address));
                }

                if (segment.Address > lastAddress)
                {
                    throw new MmuAddressSegmentGapException(lastAddress, segment.Address);
                }

                if (segment.Address < lastAddress)
                {
                    throw new MmuAddressSegmentOverlapException(segment.Address, lastAddress);
                }

                lastAddress += segment.Length;
            }

            if (lastAddress < ushort.MaxValue)
            {
                throw new MmuAddressSegmentGapException(lastAddress, ushort.MaxValue);
            }
        }

        private static bool TriggerWriteEventForMemoryBankType(MemoryBankType type)
        {
            return type == MemoryBankType.RandomAccessMemory;
        }
    }
}
