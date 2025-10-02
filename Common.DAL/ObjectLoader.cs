namespace Common.DAL
{
    internal class ObjectLoader
    {
        protected object[] Values { get; set; }
        protected int Index { get; set; }

        public ObjectLoader(object[] values)
        {
            Values = values;
        }

        internal void ValidateEnd()
        {
            if (Index < Values.Length)
            {
                throw new IndexOutOfRangeException("More values in DAL object - check DAL object vs database table");
            }
        }

        protected void ValidateIndex()
        {
            if (Index > Values.Length - 1)
            {
                throw new IndexOutOfRangeException("Out of range - check DAL object vs database table");
            }
        }

        //RSH 2/12/24 - I realize that this is the expansion of what could be done with Load<T> but this has a much higher performance than the generic version
        internal int LoadInt()
        {
            ValidateIndex();
            int value = Values[Index] == DBNull.Value ? 0 : (int)Values[Index];
            Index++;
            return value;
        }
        //RSH 2/12/24 - I realize that this is the expansion of what could be done with Load<T> but this has a much higher performance than the generic version
        internal string LoadString()
        {
            ValidateIndex();
            string value = Values[Index] == DBNull.Value ? null : (string)Values[Index];
            Index++;
            return value;
        }
        //RSH 2/12/24 - I realize that this is the expansion of what could be done with Load<T> but this has a much higher performance than the generic version
        internal DateTimeOffset LoadDateTimeOffset()
        {
            ValidateIndex();
            DateTimeOffset value = Values[Index] == DBNull.Value ? DateTimeOffset.MinValue : (DateTimeOffset)Values[Index];
            Index++;
            return value;
        }
        //RSH 2/12/24 - I realize that this is the expansion of what could be done with Load<T> but this has a much higher performance than the generic version
        internal bool LoadBoolean()
        {
            ValidateIndex();
            bool value = Values[Index] == DBNull.Value ? false : (bool)Values[Index];
            Index++;
            return value;
        }
        //RSH 2/12/24 - I realize that this is the expansion of what could be done with Load<T> but this has a much higher performance than the generic version
        internal int? LoadNullableInt()
        {
            ValidateIndex();
            int? value = Values[Index] == DBNull.Value ? null : (int)Values[Index];
            Index++;
            return value;
        }
        //RSH 2/12/24 - I realize that this is the expansion of what could be done with Load<T> but this has a much higher performance than the generic version
        internal DateTimeOffset? LoadNullableDateTimeOffset()
        {
            ValidateIndex();
            DateTimeOffset? value = Values[Index] == DBNull.Value ? null : (DateTimeOffset)Values[Index];
            Index++;
            return value;
        }
        //RSH 2/12/24 - I realize that this is the expansion of what could be done with Load<T> but this has a much higher performance than the generic version
        internal bool? LoadNullableBoolean()
        {
            ValidateIndex();
            bool? value = Values[Index] == DBNull.Value ? null : (bool)Values[Index];
            Index++;
            return value;
        }
    }
}