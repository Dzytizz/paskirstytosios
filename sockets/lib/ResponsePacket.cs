namespace lib
{
    [Serializable]
    public class ResponsePacket
    {
        public int SequenceNumber { get; set; }
        public byte[] Bytes { get; set; }

        public ResponsePacket()
        {
            Bytes = new byte[0];
        }

        public ResponsePacket(int sequenceNumber, byte[] bytes)
        {
            SequenceNumber = sequenceNumber;
            Bytes = bytes;
        }
    }
}
