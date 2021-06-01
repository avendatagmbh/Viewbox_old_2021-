namespace Ude.Core
{
    /// <summary>
    ///   Parallel state machine for the Coding Scheme Method
    /// </summary>
    public class CodingStateMachine
    {
        private readonly SMModel model;
        private int currentBytePos;
        private int currentCharLen;
        private int currentState;

        public CodingStateMachine(SMModel model)
        {
            currentState = SMModel.START;
            this.model = model;
        }

        public int CurrentCharLen
        {
            get { return currentCharLen; }
        }

        public string ModelName
        {
            get { return model.Name; }
        }

        public int NextState(byte b)
        {
            // for each byte we get its class, if it is first byte, 
            // we also get byte length
            int byteCls = model.GetClass(b);
            if (currentState == SMModel.START)
            {
                currentBytePos = 0;
                currentCharLen = model.charLenTable[byteCls];
            }

            // from byte's class and stateTable, we get its next state            
            currentState = model.stateTable.Unpack(
                currentState*model.ClassFactor + byteCls);
            currentBytePos++;
            return currentState;
        }

        public void Reset()
        {
            currentState = SMModel.START;
        }
    }
}