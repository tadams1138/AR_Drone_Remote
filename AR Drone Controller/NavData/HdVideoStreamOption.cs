using System;

namespace AR_Drone_Controller.NavData
{
    public class HdVideoStreamOption
    {
        [Flags]
        private enum hdvideo_states : uint
        {
            NAVDATA_HDVIDEO_STORAGE_FIFO_IS_FULL = (1 << 0),
            NAVDATA_HDVIDEO_USBKEY_IS_PRESENT = (1 << 8),
            NAVDATA_HDVIDEO_USBKEY_IS_RECORDING = (1 << 9),
            NAVDATA_HDVIDEO_USBKEY_IS_FULL = (1 << 10)
        };

        public uint hdvideo_state { get; internal set; }
        public uint storage_fifo_nb_packets { get; internal set; }
        public uint storage_fifo_size { get; internal set; }
        public uint usbkey_size { get; internal set; } /*! USB key in kbytes - 0 if no key present */
        public uint usbkey_freespace { get; internal set; } /*! USB key free space in kbytes - 0 if no key present */
        public uint frame_number { get; internal set; } /*! 'frame_number' PaVE field of the frame starting to be encoded for the HD stream */
        public uint usbkey_remaining_time { get; internal set; } /*! time in seconds */

        public virtual bool UsbKeyIsRecording { get { return (hdvideo_state & (uint)hdvideo_states.NAVDATA_HDVIDEO_USBKEY_IS_RECORDING) > 0; } }
        public bool UsbKeyIsPresent { get { return (hdvideo_state & (uint)hdvideo_states.NAVDATA_HDVIDEO_USBKEY_IS_PRESENT) > 0; } }
        public bool StorageFifoIsFull { get { return (hdvideo_state & (uint)hdvideo_states.NAVDATA_HDVIDEO_STORAGE_FIFO_IS_FULL) > 0; } }
        public bool UsbKeyIsFull { get { return (hdvideo_state & (uint)hdvideo_states.NAVDATA_HDVIDEO_USBKEY_IS_FULL) > 0; } }
        public virtual bool CanRecord
        {
            get
            {
                return UsbKeyIsPresent && !UsbKeyIsFull;
            }
        }

        internal static HdVideoStreamOption FromReader(ushort size, System.IO.BinaryReader reader)
        {
            Validate(size);
            var result = new HdVideoStreamOption
            {
                hdvideo_state = reader.ReadUInt32(),
                storage_fifo_nb_packets = reader.ReadUInt32(),
                storage_fifo_size = reader.ReadUInt32(),
                usbkey_size = reader.ReadUInt32(),
                usbkey_freespace = reader.ReadUInt32(),
                frame_number = reader.ReadUInt32(),
                usbkey_remaining_time = reader.ReadUInt32()
            };

            return result;
        }

        private static void Validate(ushort size)
        {
            // TODO
        }
    }
}
