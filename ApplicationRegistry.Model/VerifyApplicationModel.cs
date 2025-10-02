namespace ApplicationRegistry.Model
{
    public class ApplicationVerificationStatus
    {
        public VerificationStatusTypeEnum StatusType { get; set; }
    }
    public enum VerificationStatusTypeEnum
    {
        UnknownApplication,
        NotActiveApplication,
        InvalidApplicationData,
        AccessAllowed
    }
    public class VerifyApplicationModel
    {
        public int ApplicationInstanceID { get; set; }
        public string ApplicationName { get; set; }
        public int ApplicationVersionID { get; set; }
        public string ApplicationVersion { get; set; }

        public string AccessToFriendlyName { get; set; }
        public string Hash { get; set; }
        public string URLPort { get; set; }

        //public override bool Equals(object? obj)
        //{
        //    return base.Equals(obj);
        //}
        //public bool Equals(VerifyApplicationModel obj)
        //{
        //    if(obj==null)
        //    {
        //        return false;
        //    }
        //    if (this.ApplicationInstanceID == obj.ApplicationInstanceID)
        //    {
        //        if (string.Equals(this.ApplicationName, obj.ApplicationName, StringComparison.OrdinalIgnoreCase))
        //        {
        //            if (this.ApplicationVersionID == obj.ApplicationVersionID)
        //            {
        //                if (string.Equals(this.ApplicationVersion, obj.ApplicationVersion, StringComparison.OrdinalIgnoreCase))
        //                {
        //                    if (string.Equals(this.AccessToFriendlyName, obj.AccessToFriendlyName, StringComparison.OrdinalIgnoreCase))
        //                    {
        //                        if (string.Equals(this.Hash, obj.Hash))
        //                        {
        //                            if (string.Equals(this.URLPort, obj.URLPort, StringComparison.OrdinalIgnoreCase))
        //                            {
        //                                return true;
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    return false;
        //}

    }
}
