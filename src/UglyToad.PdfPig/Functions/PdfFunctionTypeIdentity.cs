namespace UglyToad.PdfPig.Functions
{
    using System;
    using UglyToad.PdfPig.Tokens;

    internal sealed class PdfFunctionTypeIdentity : PdfFunction
    {
        public PdfFunctionTypeIdentity(DictionaryToken function) : base((DictionaryToken)null)
        {
            //TODO passing null is not good because getCOSObject() can result in an NPE in the base class
        }

        public override int FunctionType
        {
            get
            {
                // shouldn't be called
                throw new NotSupportedException("PdfFunctionTypeIdentity");
                //TODO this is a violation of the interface segregation principle
            }
        }

        public override double[] Eval(params double[] input)
        {
            return input;
        }

        protected override ArrayToken RangeValues
        {
            get
            {
                return null;
            }
        }
    }
}
