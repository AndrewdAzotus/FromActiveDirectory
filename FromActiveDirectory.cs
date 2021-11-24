
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices;

namespace AdFns
{
    public class FromActiveDirectory
    {
        /// <summary>
        /// Usage:
        ///     Initialise:
        ///         FromActiveDirectory userList = new FromActiveDirectory();
        ///         
        ///     get full name [display name]
        ///         string userName = userList.FullName(userID);
        ///     where userID is the user's AD id with or without the domain
        /// 
        /// </summary>

        class UserInfo
        {
            string userid;
            string fullName;
            string emailAddr;
            List<string> lstGroups;

            

            public UserInfo(string userid, string fltrGroupPrefix = "")
            {

                if (userid.Contains(@"\")) userid = userid.Substring(userid.IndexOf(@"\") + 1);
                this.userid = userid;
                UserPrincipal userInfo = UserPrincipal.FindByIdentity(new PrincipalContext(ContextType.Domain), userid);
                this.fullName = userInfo.DisplayName;
                this.emailAddr = userInfo.EmailAddress;

                // DirectoryEntry directoryEntry = (DirectoryEntry)userInfo.GetUnderlyingObject();
                // 
                // foreach (var thingy in directoryEntry.Properties)
                // {
                //     string fred = ((PropertyValueCollection)thingy)[0].ToString();
                // }
                // 
                // UserPrincipal manager = (UserPrincipal)directoryEntry.Properties["manager"][0];
                // UserPrincipal mgr = UserPrincipal.FindByIdentity(new PrincipalContext(ContextType.Domain), IdentityType.DistinguishedName, directoryEntry.Properties["manager"][0].ToString());

                // Warning, this takes a REALLY long time
                if (!string.IsNullOrEmpty(fltrGroupPrefix))
                {
                    int fltrLength = fltrGroupPrefix.Length;
                    LstGroups = new List<string>();
                    foreach (GroupPrincipal group in userInfo.GetGroups())
                        if (fltrLength <= group.Name.Length && fltrGroupPrefix == group.Name.Substring(0, fltrLength))
                            lstGroups.Add(group.Name);
                }
            }

            internal string Userid { get => userid; set => userid = value; }
            internal string FullName { get => fullName; set => fullName = value; }
            internal string EmailAddr { get => emailAddr; set => emailAddr = value; }
            public List<string> LstGroups { get => lstGroups; set => lstGroups = value; }
        }

        Dictionary<string, UserInfo> users = new Dictionary<string, UserInfo>();

        public FromActiveDirectory()
        {

        }

        public string EmailAddr(string userid)
        {
            string rc = "";

            if (!string.IsNullOrEmpty(userid))
            {
                if (userid.Contains(@"\")) userid = userid.Substring(userid.IndexOf(@"\") + 1);
                // I know this looks weird, but this code is desgned to be aSync'd
                //   and the call to UserInfo can take a long time, esp. if groups
                //   are requested below. If two calls are initiated for the same
                //   ID then one can fail if it is aSync'd because the keys are not
                //   updated quickly enough
                if (!users.ContainsKey(userid))
                    users[userid] = null;
                if (users[userid] == null)
                    users[userid] = new UserInfo(userid);
                rc = users[userid].EmailAddr;
            }

            return rc;
        }

        public string FullName(string userid)
        {
            string rc = "";

            if (!string.IsNullOrEmpty(userid))
            {
                if (userid.Contains(@"\")) userid = userid.Substring(userid.IndexOf(@"\") + 1);
                if (!users.ContainsKey(userid))
                    users[userid] = null;
                if (users[userid] == null)
                    users[userid] = new UserInfo(userid);
                rc = users[userid].FullName;
            }

            return rc;
        }

        public List<string> Groups(string userid, string prfx)
        {
            List<string> rc = new List<string>();

            if (!string.IsNullOrEmpty(userid))
            {
                if (userid.Contains(@"\")) userid = userid.Substring(userid.IndexOf(@"\") + 1);
                if (!users.ContainsKey(userid))
                    users[userid] = null;
                if (users[userid] == null || users[userid].LstGroups==null)
                    users[userid] = new UserInfo(userid, prfx);
                rc = users[userid].LstGroups;
            }

            return rc;
        }

        private void RetrieveUserInfo(string userid)
        {

        }
    }
}