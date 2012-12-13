using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.DirectoryServices;
using System.Management;

namespace JureHR
{
    /// <summary>
    /// Active Directory and Windows Accounts
    /// </summary>
    public class ActiveDirectory
    {
        /// <summary>
        /// Active Directory Constructor
        /// </summary>
        public ActiveDirectory()
        {
        }

        /// <summary>
        /// Get all accounts on local system with WMI
        /// </summary>
        /// <returns>populates a generic list with the Windows accounts on the local machine</returns>
        public List<string> GetWindowsAccounts()
        {
            List<string> accounts = new List<string>();
            //set the scope of this search to the local machine
            ManagementScope scope = new ManagementScope("\\\\" + Environment.MachineName + "\\root\\cimv2");
            //connect to the machine
            scope.Connect();

            //use a SelectQuery to tell what we're searching in
            SelectQuery searchQuery = new SelectQuery("select * from Win32_UserAccount where Domain='" + Environment.MachineName + "'");

            //set the search up
            ManagementObjectSearcher searcherObj = new ManagementObjectSearcher(scope, searchQuery);


            //loop through the collection looking for the account name
            foreach (ManagementObject obj in searcherObj.Get())
            {
                accounts.Add(obj["Name"].ToString());
            }

            return accounts;
        }

        /// <summary>
        /// add a user to a group
        /// </summary>
        /// <param name="userDN"></param>
        /// <param name="groupDN"></param>
        /// <returns></returns>
        public bool AddToGroup(string userDN, string groupDN)
        {
            try
            {
                DirectoryEntry de = new DirectoryEntry("LDAP://" + groupDN);

                de.Properties["member"].Add(userDN);
                de.CommitChanges();
                de.Close();
                return true;
            }
            catch (Exception ex)
            {
                Mailer.ErrNotify(ex, "ActiveDirectory addToGroup");
                return false;
            }
        }

        /// <summary>
        /// change a users password.
        /// </summary>
        /// <param name="userDn"></param>
        /// <param name="oldPassword"></param>
        /// <param name="newPassword"></param>
        /// <returns></returns>
        public bool ChangePassword(string userDn, string oldPassword, string newPassword)
        {
            try
            {
                DirectoryEntry uEntry = new DirectoryEntry("LDAP://" + userDn);
                uEntry.Invoke("ChangePassword", new object[] { oldPassword, newPassword });
                uEntry.Properties["LockOutTime"].Value = 0; //unlock account
                uEntry.CommitChanges();
                uEntry.Close();
                return true;
            }
            catch (Exception ex)
            {
                Mailer.ErrNotify(ex, "ActiveDirectory ChangePassword");
                return false;
            }
        }

        /// <summary>
        /// find user by cn
        /// </summary>
        /// <param name="cn"></param>
        /// <param name="baseDN"></param>
        /// <returns></returns>
        public bool CnExists(string cn, string baseDN)
        {
            DirectoryEntry entry = GetDE(baseDN);

            try
            {
                DirectorySearcher search = new DirectorySearcher(entry);
                search.Filter = "(cn=" + cn + ")";
                SearchResult result = search.FindOne();

                if (result != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Mailer.ErrNotify(ex, "ActiveDirectory CnExists");
                return false;
            }
        }

        /// <summary>
        /// create a user account
        /// </summary>
        /// <param name="parentOUDN"></param>
        /// <param name="samName"></param>
        /// <param name="userPassword"></param>
        /// <param name="firstName"></param>
        /// <param name="lastName"></param>
        /// <returns></returns>
        public bool CreateUserAccount(string parentOUDN, string samName, string userPassword, string firstName, string lastName)
        {
            try
            {
                string connectionPrefix = "LDAP://" + parentOUDN;
                DirectoryEntry de = new DirectoryEntry(connectionPrefix);
                DirectoryEntry newUser = de.Children.Add("CN=" + firstName + " " + lastName, "user");
                newUser.Properties["samAccountName"].Value = samName;
                newUser.Properties["userPrincipalName"].Value = samName;
                newUser.Properties["sn"].Add(lastName);
                newUser.Properties["name"].Value = firstName + " " + lastName;
                newUser.Properties["givenName"].Add(firstName);


                newUser.CommitChanges();
                newUser.Invoke("SetPassword", new object[] { userPassword });
                newUser.CommitChanges();
                int val = (int)newUser.Properties["userAccountControl"].Value;
                newUser.Properties["userAccountControl"].Value = val | 0x0200;
                newUser.CommitChanges();
                de.Close();
                newUser.Close();
                return true;
            }
            catch (Exception ex)
            {
                Mailer.ErrNotify(ex, "ActiveDirectory CreateUserAccount");
                return false;
            }
            /*
                //Add this to the create account method
                int val = (int)newUser.Properties["userAccountControl"].Value; 
                     //newUser is DirectoryEntry object
                newUser.Properties["userAccountControl"].Value = val | 0x80000; 
                    //ADS_UF_TRUSTED_FOR_DELEGATION
                 
             * 
             * UserAccountControlFlags
             * CONST   HEX
                -------------------------------
                SCRIPT 0x0001
                ACCOUNTDISABLE 0x0002
                HOMEDIR_REQUIRED 0x0008
                LOCKOUT 0x0010
                PASSWD_NOTREQD 0x0020
                PASSWD_CANT_CHANGE 0x0040
                ENCRYPTED_TEXT_PWD_ALLOWED 0x0080
                TEMP_DUPLICATE_ACCOUNT 0x0100
                NORMAL_ACCOUNT 0x0200
                INTERDOMAIN_TRUST_ACCOUNT 0x0800
                WORKSTATION_TRUST_ACCOUNT 0x1000
                SERVER_TRUST_ACCOUNT 0x2000
                DONT_EXPIRE_PASSWORD 0x10000
                MNS_LOGON_ACCOUNT 0x20000
                SMARTCARD_REQUIRED 0x40000
                TRUSTED_FOR_DELEGATION 0x80000
                NOT_DELEGATED 0x100000
                USE_DES_KEY_ONLY 0x200000
                DONT_REQ_PREAUTH 0x400000
                PASSWORD_EXPIRED 0x800000
                TRUSTED_TO_AUTH_FOR_DELEGATION 0x1000000
             * */
        }

        /// <summary>
        /// disable an account
        /// </summary>
        /// <param name="userDn"></param>
        /// <returns></returns>
        public bool DisableAccount(string userDn)
        {
            try
            {
                DirectoryEntry user = new DirectoryEntry(userDn);
                int val = (int)user.Properties["userAccountControl"].Value;
                user.Properties["userAccountControl"].Value = val | 0x2;
                //ADS_UF_ACCOUNTDISABLE;
                user.CommitChanges();
                user.Close();
                return true;
            }
            catch (Exception ex)
            {
                Mailer.ErrNotify(ex, "ActiveDirectory DisableAccount");
                return false;
            }
        }

        /// <summary>
        /// return samName from employeeID
        /// </summary>
        /// <param name="empID"></param>
        /// <returns></returns>
        public string EmpIDtoSamName(string empID)
        {
            DirectoryEntry entry = GetDE();

            try
            {
                DirectorySearcher search = new DirectorySearcher(entry);
                search.Filter = "(employeeID=" + empID + ")";
                SearchResult result = search.FindOne();

                if (result != null)
                {

                    return result.Properties["SAMAccountName"][0].ToString();
                }
                else
                {
                    return "Could not find any employee with that ID#";
                }
            }
            catch (Exception ex)
            {
                Mailer.ErrNotify(ex, "ActiveDirectory EmpIDtoSamName");
                return null;
            }
        }

        /// <summary>
        /// enable a user account
        /// </summary>
        /// <param name="userDn"></param>
        /// <returns></returns>
        public bool EnableAccount(string userDn)
        {
            try
            {
                DirectoryEntry user = new DirectoryEntry("LDAP://" + userDn);
                user.Properties["userAccountControl"].Value = 0x200;
                //ADS_UF_NORMAL_ACCOUNT;
                user.CommitChanges();
                user.Close();
                return true;
            }
            catch (Exception ex)
            {
                Mailer.ErrNotify(ex, "ActiveDirectory EnableAccount");
                return false;
            }
        }

        /// <summary>
        /// generate password based on seed string
        /// </summary>
        /// <param name="seed"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public string GeneratePassword(string seed, int length)
        {
            string input = seed;
            string rndNum, specChar;

            Random rnd = new Random();
            int num1 = rnd.Next(0, 9);
            int num2 = rnd.Next(0, 9);

            switch (num2)
            {
                case 0:
                    specChar = "!";
                    break;
                case 1:
                    specChar = "@";
                    break;
                case 2:
                    specChar = "#";
                    break;
                case 3:
                    specChar = "$";
                    break;
                case 4:
                    specChar = "%";
                    break;
                case 5:
                    specChar = "^";
                    break;
                case 6:
                    specChar = "&";
                    break;
                case 7:
                    specChar = "*";
                    break;
                case 8:
                    specChar = "(";
                    break;
                case 9:
                    specChar = ")";
                    break;
                default:
                    specChar = "!";
                    break;
            }

            rndNum = num1.ToString();
            input = specChar + input + num1;

            while (input.Length < length)
            {
                input += "0";
            }

            input = input.Replace('a', '@');
            input = input.Replace('d', 'D');
            input = input.Replace('i', '!');
            input = input.Replace('l', '1');
            input = input.Replace('o', '0');
            input = input.Replace('O', '0');
            input = input.Replace('s', '$');
            input = input.Replace('q', 'Q');
            input = input.Replace('b', 'B');
            input = input.Replace('e', '3');
            input = input.Replace('h', 'H');
            input = input.Replace('n', 'N');

            return input;
        }

        /// <summary>
        /// generate password of given length
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public string GeneratePassword(int length)
        {
            string pass = "";

            Random rnd = new Random();
            int ch;
            while (pass.Length < length)
            {
                ch = rnd.Next(32, 127);
                pass += Convert.ToChar(ch);
            }
            return pass;
        }

        /// <summary>
        /// create directory entry object
        /// </summary>
        /// <returns></returns>
        public DirectoryEntry GetDE()
        {
            DirectoryEntry de = new DirectoryEntry("LDAP://" + GetDomainDN());
            de.AuthenticationType = AuthenticationTypes.Secure;

            return de;
        }

        /// <summary>
        /// create directory entry object
        /// </summary>
        /// <param name="baseDN"></param>
        /// <returns></returns>
        public DirectoryEntry GetDE(string baseDN)
        {
            DirectoryEntry de = new DirectoryEntry("LDAP://" + baseDN);
            de.AuthenticationType = AuthenticationTypes.Secure;

            return de;
        }

        /// <summary>
        /// enum computers in domain
        /// </summary>
        /// <returns></returns>
        public SearchResultCollection GetDomainComputers()
        {
            DirectorySearcher deSearch = new DirectorySearcher("LDAP://" + GetDomainDN());
            deSearch.Filter = "(objectClass=Computer)";
            deSearch.SearchScope = SearchScope.Subtree;
            SearchResultCollection results = deSearch.FindAll();

            return results;
        }

        /// <summary>
        /// enum computers in domain
        /// </summary>
        /// <returns></returns>
        public SearchResultCollection GetOUComputers(string ouDN)
        {
            DirectoryEntry entry = GetDE(ouDN);

            DirectorySearcher deSearch = new DirectorySearcher(entry);
            deSearch.Filter = "(objectClass=Computer)";
            deSearch.SearchScope = SearchScope.OneLevel;
            SearchResultCollection results = deSearch.FindAll();

            return results;
        }

        /// <summary>
        /// gets the distinguished name of your domain
        /// </summary>
        /// <returns></returns>
        private string GetDomainDN()
        {
            try
            {
                DirectoryEntry de = new DirectoryEntry("LDAP://RootDSE");
                de.AuthenticationType = AuthenticationTypes.Secure;

                return de.Properties["defaultnamingcontext"][0].ToString();
            }
            catch (Exception ex)
            {

                Mailer.ErrNotify(ex, "ActiveDirectory GetDomainDN");
                return null;
            }
        }

        /// <summary>
        /// find user by sam name.
        /// </summary>
        /// <param name="samAccountName"></param>
        /// <returns></returns>
        public string GetUserDN(string samAccountName)
        {
            DirectoryEntry entry = GetDE();
            String account = samAccountName.Replace(@"Domain\", "");

            try
            {
                DirectorySearcher search = new DirectorySearcher(entry);
                search.Filter = "(SAMAccountName=" + account + ")";
                SearchResult result = search.FindOne();

                if (result != null)
                {
                    return result.Properties["distinguishedName"][0].ToString();
                }
                else
                {
                    return "";
                }
            }
            catch (Exception ex)
            {
                Mailer.ErrNotify(ex, "ActiveDirectory GetUserDN");
                return "";
            }
        }

        /// <summary>
        /// increments a number on the end of a given samName until it is unique
        /// </summary>
        /// <param name="samInitial"></param>
        /// <returns></returns>
        public string IncName(string samInitial)
        {
            if (SamExists(samInitial) == true)
            {
                int i = 1;
                string newSam = samInitial;
                while (SamExists(newSam))
                {
                    newSam = samInitial + i.ToString();
                    Console.WriteLine(newSam);
                    Console.Read();
                    i++;
                }

                return newSam;
            }
            else
            {
                return samInitial;
            }
        }

        /// <summary>
        /// list top-level ous
        /// </summary>
        /// <returns></returns>
        public List<string> ListOUs()
        {
            DirectoryEntry entry = GetDE();
            List<string> ous = new List<string>();
            try
            {
                DirectorySearcher search = new DirectorySearcher(entry);
                search.SearchScope = SearchScope.OneLevel;
                search.Filter = "(objectCategory=organizationalUnit)";
                SearchResultCollection results = search.FindAll();

                for (int i = 0; i < results.Count; i++)
                {
                    ous.Add(results[i].Properties["ADsPath"][0].ToString());
                }
                return ous;
            }
            catch (Exception ex)
            {

                Mailer.ErrNotify(ex, "ActiveDirectory ListOUs");
                return null;
            }
        }

        /// <summary>
        /// list Users in directory
        /// </summary>
        /// <returns></returns>
        public List<string> ListUsers()
        {
            DirectoryEntry entry = GetDE();
            List<string> allUsers = new List<string>();

            DirectorySearcher search = new DirectorySearcher(entry);
            search.Filter = "(&(objectClass=user)(objectCategory=person))";
            search.PropertiesToLoad.Add("samaccountname");
            search.PropertiesToLoad.Add("cn"); // username
            search.PropertiesToLoad.Add("name"); // full name
            search.PropertiesToLoad.Add("givenname"); // firstname
            search.PropertiesToLoad.Add("sn"); // lastname
            search.PropertiesToLoad.Add("mail"); // mail
            search.PropertiesToLoad.Add("initials"); // initials
            search.PropertiesToLoad.Add("ou"); // organizational unit
            search.PropertiesToLoad.Add("userPrincipalName"); // login name
            search.PropertiesToLoad.Add("distinguishedName"); // distinguised name

            SearchResult result;
            SearchResultCollection resultCol = search.FindAll();
            if (resultCol != null)
            {
                for (int counter = 0; counter < resultCol.Count; counter++)
                {
                    result = resultCol[counter];
                    if (result.Properties.Contains("samaccountname"))
                    {
                        allUsers.Add((String)result.Properties["samaccountname"][0]);
                    }
                }
            }
            return allUsers;
        }

        /// <summary>
        /// list child ous of an object
        /// </summary>
        /// <param name="baseDN"></param>
        /// <returns></returns>
        public List<string> ListOUs(string baseDN)
        {
            DirectoryEntry entry = GetDE(baseDN);
            List<string> ous = new List<string>();
            try
            {
                DirectorySearcher search = new DirectorySearcher(entry);
                search.SearchScope = SearchScope.OneLevel;
                search.Filter = "(objectCategory=organizationalUnit)";
                SearchResultCollection results = search.FindAll();

                for (int i = 0; i < results.Count; i++)
                {
                    ous.Add(results[i].Properties["ADsPath"][0].ToString());
                }
                return ous;
            }
            catch (Exception ex)
            {
                Mailer.ErrNotify(ex, "ActiveDirectory ListOUs");
                return null;
            }
        }

        /// <summary>
        /// move an obect in active directory
        /// </summary>
        /// <param name="objectDN"></param>
        /// <param name="targetDN"></param>
        /// <returns></returns>
        public bool MoveObject(string objectDN, string targetDN)
        {
            try
            {
                DirectoryEntry eLocation = new DirectoryEntry("LDAP://" + objectDN);
                DirectoryEntry nLocation = new DirectoryEntry("LDAP://" + targetDN);
                string newName = eLocation.Name;
                eLocation.MoveTo(nLocation, newName);
                nLocation.Close();
                eLocation.Close();
                return true;
            }
            catch (Exception ex)
            {
                Mailer.ErrNotify(ex, "ActiveDirectory MoveObject");
                return false;
            }
        }

        /// <summary>
        /// remove a user from a group
        /// </summary>
        /// <param name="userDN"></param>
        /// <param name="groupDN"></param>
        /// <returns></returns>
        public bool RemoveUserFromGroup(string userDN, string groupDN)
        {
            try
            {
                DirectoryEntry dirEntry = new DirectoryEntry("LDAP://" + groupDN);
                dirEntry.Properties["member"].Remove(userDN);
                dirEntry.CommitChanges();
                dirEntry.Close();
                return true;
            }
            catch (Exception ex)
            {
                Mailer.ErrNotify(ex, "ActiveDirectory RemoveUserFromGroup");
                return false;
            }
        }

        /// <summary>
        /// rename an AD object
        /// </summary>
        /// <param name="objectDn"></param>
        /// <param name="newName"></param>
        /// <returns></returns>
        public bool RenameObject(string objectDn, string newName)
        {
            try
            {
                DirectoryEntry child = new DirectoryEntry("LDAP://" + objectDn);
                child.Rename("CN=" + newName);
                return true;
            }
            catch (Exception ex)
            {

                Mailer.ErrNotify(ex, "ActiveDirectory RenameObject");
                return false;
            }
        }

        /// <summary>
        /// reset a users password
        /// </summary>
        /// <param name="userDn"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public bool ResetPassword(string userDn, string password)
        {
            try
            {
                DirectoryEntry uEntry = new DirectoryEntry("LDAP://" + userDn);
                uEntry.Invoke("SetPassword", new object[] { password });
                uEntry.Properties["LockOutTime"].Value = 0; //unlock account
                uEntry.CommitChanges();
                uEntry.Close();
                return true;
            }
            catch (Exception ex)
            {
                Mailer.ErrNotify(ex, "ActiveDirectory ResetPassword");
                return false;
            }
        }

        /// <summary>
        /// get distinguished name of a group
        /// </summary>
        /// <param name="groupName"></param>
        /// <returns></returns>
        public string ReturnGroupDN(string groupName)
        {
            DirectoryEntry de = GetDE();
            DirectorySearcher ds = new DirectorySearcher(de);
            ds.Filter = ("(&(SAMAccountName=" + groupName + "))");
            ds.SearchScope = SearchScope.Subtree;
            SearchResult results = ds.FindOne();

            if (results != null)
            {
                try
                {
                    DirectoryEntry user = results.GetDirectoryEntry();
                    return user.Properties["distinguishedName"].Value.ToString();
                }
                catch (Exception ex)
                {
                    Mailer.ErrNotify(ex, "ActiveDirectory ReturnGroupDN");
                    return null;
                }
            }
            return null;
        }

        /// <summary>
        /// get AD property
        /// </summary>
        /// <param name="objectDN"></param>
        /// <param name="propertyName"></param>
        /// <param name="execute"></param>
        /// <returns></returns>
        public string ReturnProperty(string objectDN, string propertyName, bool execute)
        {
            DirectoryEntry de = GetDE();
            DirectorySearcher ds = new DirectorySearcher(de);
            ds.Filter = ("(distinguishedName=" + objectDN + ")");
            ds.SearchScope = SearchScope.Subtree;
            SearchResult results = ds.FindOne();

            if (results != null)
            {
                try
                {
                    DirectoryEntry user = results.GetDirectoryEntry();
                    return user.Properties[propertyName].Value.ToString();
                }
                catch (Exception ex)
                {
                    Mailer.ErrNotify(ex, "ActiveDirectory ReturnProperty");
                    return null;
                }
            }
            return null;
        } 

        /// <summary>
        /// get AD property
        /// </summary>
        /// <param name="samAccountName"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public string ReturnProperty(string samAccountName, string propertyName)
        {
            DirectoryEntry de = GetDE();
            DirectorySearcher ds = new DirectorySearcher(de);
            ds.Filter = ("(&(objectclass=user)(objectcategory=person)(SAMAccountName=" + samAccountName + "))");
            ds.SearchScope = SearchScope.Subtree;
            SearchResult results = ds.FindOne();

            if (results != null)
            {
                try
                {
                    DirectoryEntry user = results.GetDirectoryEntry();
                    return user.Properties[propertyName].Value.ToString();
                }
                catch (Exception ex)
                {
                    Mailer.ErrNotify(ex, "ActiveDirectory ReturnProperty");
                    return null;
                }
            }
            return null;
        }

        /// <summary>
        /// return ad properties of an object
        /// </summary>
        /// <param name="objectDN"></param>
        /// <returns></returns>
        public List<string> ReturnPropertyNames(string objectDN)
        {
            DirectoryEntry de = GetDE();
            DirectorySearcher ds = new DirectorySearcher(de);
            ds.Filter = ("(distinguishedName=" + objectDN + ")");
            ds.SearchScope = SearchScope.Subtree;
            SearchResult results = ds.FindOne();
            List<string> props = new List<string>();
            if (results != null)
            {
                try
                {
                    DirectoryEntry user = results.GetDirectoryEntry();
                    //build the string[] of properties

                    foreach (string name in user.Properties.PropertyNames)
                    {
                        props.Add(name);
                    }
                    return props;
                }
                catch (Exception ex)
                {
                    Mailer.ErrNotify(ex, "ActiveDirectory ReturnPropertyNames");
                    return null;
                }
            }
            return null;
        }

        /// <summary>
        /// return a property by cn
        /// </summary>
        /// <param name="cn"></param>
        /// <param name="propertyName"></param>
        /// <param name="baseOUDN"></param>
        /// <returns></returns>
        public string ReturnPropertyByCN(string cn, string propertyName, string baseOUDN)
        {
            DirectoryEntry de = GetDE();
            DirectorySearcher ds = new DirectorySearcher(de);
            ds.Filter = ("(&(objectclass=user)(objectcategory=person)(cn=" + cn + "))");
            ds.SearchRoot = GetDE(baseOUDN);
            ds.SearchScope = SearchScope.Subtree;
            SearchResult results = ds.FindOne();

            if (results != null)
            {
                try
                {
                    DirectoryEntry user = results.GetDirectoryEntry();
                    if (user.Properties[propertyName].Value.ToString() != "Exception has been thrown by the target of an invocation.")
                    {
                        return user.Properties[propertyName].Value.ToString();
                    }
                    else
                    {
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    Mailer.ErrNotify(ex, "ActiveDirectory ReturnPropertyByCN");
                    return null;
                }
            }
            return null;
        }

        /// <summary>
        /// find user by sam name.
        /// </summary>
        /// <param name="samAccountName"></param>
        /// <returns></returns>
        public bool SamExists(string samAccountName)
        {
            DirectoryEntry entry = GetDE();
            String account = samAccountName.Replace(@"Domain\", "");

            try
            {
                DirectorySearcher search = new DirectorySearcher(entry);
                search.Filter = "(SAMAccountName=" + account + ")";
                SearchResult result = search.FindOne();

                if (result != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Mailer.ErrNotify(ex, "ActiveDirectory SamExists");
                return false;
            }
        }

        /// <summary>
        /// set value of property
        /// </summary>
        /// <param name="samAccountName"></param>
        /// <param name="propertyName"></param>
        /// <param name="newValue"></param>
        /// <returns></returns>
        public bool SetProperty(string samAccountName, string propertyName, string newValue)
        {
            DirectoryEntry de = GetDE();
            DirectorySearcher ds = new DirectorySearcher(de);
            ds.Filter = ("(&(objectclass=user)(objectcategory=person)(SAMAccountName=" + samAccountName + "))");
            ds.SearchScope = SearchScope.Subtree;
            SearchResult results = ds.FindOne();

            if (results != null)
            {
                try
                {
                    DirectoryEntry updateEntry = results.GetDirectoryEntry();
                    updateEntry.Properties[propertyName].Value = newValue;
                    updateEntry.CommitChanges();
                    updateEntry.Close();
                    return true;
                }
                catch (Exception ex)
                {
                    Mailer.ErrNotify(ex, "ActiveDirectory SetProperty");
                    return false;
                }
            }
            return false;
        }

        /// <summary>
        /// set value of property
        /// </summary>
        /// <param name="objectDN"></param>
        /// <param name="propertyName"></param>
        /// <param name="newValue"></param>
        /// <param name="execute"></param>
        /// <returns></returns>
        public bool SetProperty(string objectDN, string propertyName, string newValue, bool execute)
        {
            DirectoryEntry de = GetDE();
            DirectorySearcher ds = new DirectorySearcher(de);
            ds.Filter = ("(distinguishedName=" + objectDN + ")");
            ds.SearchScope = SearchScope.Subtree;
            SearchResult results = ds.FindOne();

            if (results != null)
            {
                try
                {
                    DirectoryEntry updateEntry = results.GetDirectoryEntry();
                    updateEntry.Properties[propertyName].Value = newValue;
                    updateEntry.CommitChanges();
                    updateEntry.Close();
                    return true;
                }
                catch (Exception ex)
                {
                    Mailer.ErrNotify(ex, "ActiveDirectory SetProperty");
                    return false;
                }
            }
            return false;
        }

        /// <summary>
        /// set value of property
        /// </summary>
        /// <param name="samAccountName"></param>
        /// <param name="propertyName"></param>
        /// <param name="newValue"></param>
        /// <returns></returns>
        public bool SetProperty(string samAccountName, string propertyName, int newValue)
        {
            DirectoryEntry de = GetDE();
            DirectorySearcher ds = new DirectorySearcher(de);
            ds.Filter = ("(&(objectclass=user)(objectcategory=person)(SAMAccountName=" + samAccountName + "))");
            ds.SearchScope = SearchScope.Subtree;
            SearchResult results = ds.FindOne();

            if (results != null)
            {
                try
                {
                    DirectoryEntry updateEntry = results.GetDirectoryEntry();
                    updateEntry.Properties[propertyName].Value = newValue;
                    updateEntry.CommitChanges();
                    updateEntry.Close();
                    return true;
                }
                catch (Exception ex)
                {
                    Mailer.ErrNotify(ex, "ActiveDirectory SetProperty");
                    return false;
                }
            }
            return false;
        }
    }
}
