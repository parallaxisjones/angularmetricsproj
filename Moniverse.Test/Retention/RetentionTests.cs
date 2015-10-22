//using System;
//using System.Collections.Generic;
//using NUnit.Framework;
//using System.Data;
//using System.Linq;
//using Moniverse.Reader;
//using System.Globalization;
//using Utilities;

//namespace Moniverse.Reader
//{

//    public class TestComp : IEqualityComparer<DataRow>
//    {

//        public bool Equals(DataRow x, DataRow y)
//        {
//            return x.Field<string>("UserId") == y.Field<string>("UserId") ;
//        }

//        public int GetHashCode(DataRow obj)
//        {
//            return 0;
//        }
//    }

//    public class MockRetentionRow : IRetentionRow {

//        public DateTime date { get; set;}
//        public int installsOnThisDay { get; set; }
//        public float[] days { get; set; }

//        public MockRetentionRow()
//        {
//            days = new float[15];
//            for (int i = days.Length -1; i > 0 ; i--)
//            {
//                days[i] = -1;
//            }
//        }
//        public void SetDayPercent(int i, float percent)
//        {
//            days[i] = percent;
//        }
//        public int GetDayNRetentionCount(DateTime today, int n)
//        {
//            int count = 0;
//            MockFirstLoginTable FirstLoginTable = new MockFirstLoginTable();
//            MockLoginTable LoginTable = new MockLoginTable();

//            DataTable flt = FirstLoginTable.GetDataTable();
//            DataTable lt = LoginTable.GetDataTable();
//            List<string> loginList = new List<string>();

//            List<string> parsedUsers = new List<string>();

//            int countOfUsers = lt.Select().Where(x => {

//                bool inFirstLogin = false;
//                bool onDate = x.Field<string>("RecordTimestamp").Contains(today.ToString("MM/dd/yyyy"));

//                if (onDate)
//                {
//                    inFirstLogin = flt.Select().Any(k =>
//                    {
//                        bool hasDate = k.Field<string>("RecordTimestamp").Contains(today.AddDays(-n).ToString("MM/dd/yyyy"));
//                        bool isSameUser = k.Field<string>("UserId") == x.Field<string>("UserId");
//                        return isSameUser && hasDate;
//                    });
//                }

//                return inFirstLogin && onDate;

//            }).Distinct(new TestComp()).Count();

//            Logger.Instance.Info(String.Format("n day count result: {0}", countOfUsers));

//            return count;
//        }
//        public int GetDayNInstallsCount(DateTime today, int n)
//        {
//            int count = 0;
//            MockFirstLoginTable FirstLoginTable = new MockFirstLoginTable();
//            try
//            {
//                count = FirstLoginTable.GetNewUsersCount(today.AddDays(-n));
//            }
//            catch (Exception ex)
//            {
//                Logger.Instance.Info(ex.Message);
//            }
//            return count;
//        }  
    
//    }

//    public class MockFirstLoginTable {

//        public DataTable Retention_FirstLogin {get;set;}

//        public MockFirstLoginTable()
//        {
//            string TwoWeeksAgo = DateTime.UtcNow.AddDays(-16).ToString("MM/dd/yyyy HH:mm:ss");
//            DataTable table = InitDataTable();

//            table.Rows.Add(1, "a", TwoWeeksAgo);
//            table.Rows.Add(2, "b", TwoWeeksAgo);
//            table.Rows.Add(3, "c", TwoWeeksAgo);
//            table.Rows.Add(4, "d", TwoWeeksAgo);
//            table.Rows.Add(5, "e", TwoWeeksAgo);
//            table.Rows.Add(6, "f", TwoWeeksAgo);
//            table.Rows.Add(7, "g", TwoWeeksAgo);
//            table.Rows.Add(8, "aa", TwoWeeksAgo);
//            table.Rows.Add(9, "bb", TwoWeeksAgo);
//            table.Rows.Add(10, "cc", TwoWeeksAgo);
//            table.Rows.Add(11, "dd", TwoWeeksAgo);
//            table.Rows.Add(12, "ee", TwoWeeksAgo);
//            table.Rows.Add(13, "ff", TwoWeeksAgo);
//            table.Rows.Add(24, "gg", TwoWeeksAgo);
//            table.Rows.Add(25, "hh", TwoWeeksAgo);
//            table.Rows.Add(26, "ii", TwoWeeksAgo);
//            table.Rows.Add(27, "kk", TwoWeeksAgo);
//            table.Rows.Add(28, "ll", TwoWeeksAgo);
//            table.Rows.Add(29, "mm", TwoWeeksAgo);
//            table.Rows.Add(30, "uu", TwoWeeksAgo);
//            table.Rows.Add(31, "tt", TwoWeeksAgo);
//            table.Rows.Add(32, "oo", TwoWeeksAgo); 
//            table.Rows.Add(33, "pp", TwoWeeksAgo); 
//            table.Rows.Add(34, "qq", TwoWeeksAgo); 
//            table.Rows.Add(35, "ww", TwoWeeksAgo); 
//            table.Rows.Add(36, "zz", TwoWeeksAgo); 



//            this.SetDataTable(table);
        
//        }

//       public DataTable InitDataTable(){


//           this.Retention_FirstLogin = new DataTable();

//           if (this.Retention_FirstLogin.Rows.Count <= 0)
//                {
//                    this.Retention_FirstLogin.Clear();
//                    this.Retention_FirstLogin.Columns.Add("ID", typeof(int));
//                    this.Retention_FirstLogin.Columns.Add("UserId", typeof(string));
//                    this.Retention_FirstLogin.Columns.Add("RecordTimestamp", typeof(string));
//                }

//           return this.Retention_FirstLogin;
//        }

//       public DataTable GetDataTable() {
//           return this.Retention_FirstLogin;
//       }

//       public void SetDataTable(DataTable dt) {
//           this.Retention_FirstLogin = dt;
//       }

//       public int GetNewUsersCount(DateTime datetime) {
           
//           if (datetime > DateTime.UtcNow)
//           {
//               Exception ex = new Exception("datetime must be before today");
//               throw ex;
//           }

//           int count = 0;
//           DataTable result = new DataTable();
//           result = this.GetDataTable();
//           //foreach (DataRow o in result.Select(String.Format("RecordTimestamp = '{0}'", datetime)))
//           //{
//           //    Logger.Instance.Info("\t" + o["RecordTimestamp"] + "\t" + o["UserId"]);
//           //}
//           count = result.Select(String.Format("RecordTimestamp = '{0}'", datetime)).Length;

//           return count;
//       }
//    }

//    public class MockLoginTable {
    
//        public DataTable UserSessionMeta {get;set;}
               
//        public MockLoginTable() {
//            DateTime TwoWeeksAgo = DateTime.UtcNow.AddDays(-14);
//            DataTable table = InitDataTable();
//            for(var i = 0; i< 15; i++){

//                string IDate = TwoWeeksAgo.AddDays(-i).ToString("MM/dd/yyyy HH:mm:ss");

//            ////should be 100 %
//                if (i == 0)
//                {
//                    table.Rows.Add(1, "a", IDate);
//                    table.Rows.Add(2, "b", IDate);
//                    table.Rows.Add(3, "c", IDate);
//                    table.Rows.Add(4, "d", IDate);
//                    table.Rows.Add(5, "e", IDate);
//                    table.Rows.Add(6, "f", IDate);
//                    table.Rows.Add(7, "g", IDate);
//                    table.Rows.Add(11, "aa", IDate);
//                    table.Rows.Add(12, "bb", IDate);
//                    table.Rows.Add(13, "cc", IDate);
//                    table.Rows.Add(14, "dd", IDate);
//                    table.Rows.Add(15, "ee", IDate);
//                    table.Rows.Add(16, "ff", IDate);
//                    table.Rows.Add(20, "gg", IDate);
//                    table.Rows.Add(21, "hh", IDate);
//                    table.Rows.Add(22, "ii", IDate);
//                    table.Rows.Add(23, "kk", IDate);
//                    table.Rows.Add(24, "ll", IDate);
//                    table.Rows.Add(25, "mm", IDate);
//                    table.Rows.Add(26, "uu", IDate);
//                    table.Rows.Add(27, "tt", IDate);
//                    table.Rows.Add(32, "oo", IDate);
//                    table.Rows.Add(33, "pp", IDate);
//                    table.Rows.Add(34, "qq", IDate);
//                    table.Rows.Add(35, "ww", IDate);
//                    table.Rows.Add(36, "zz", IDate);
//                }
//                if (i == 1)
//                {
//                    table.Rows.Add(1, "a", IDate);
//                    table.Rows.Add(2, "b", IDate);
//                    table.Rows.Add(3, "c", IDate);
//                    table.Rows.Add(4, "d", IDate);
//                    table.Rows.Add(5, "e", IDate);
//                    table.Rows.Add(6, "f", IDate);
//                    table.Rows.Add(7, "g", IDate);
//                    table.Rows.Add(11, "aa", IDate);
//                    table.Rows.Add(12, "bb", IDate);
//                    table.Rows.Add(13, "cc", IDate);
//                    table.Rows.Add(14, "dd", IDate);
//                    table.Rows.Add(15, "ee", IDate);
//                    table.Rows.Add(16, "ff", IDate);
//                    table.Rows.Add(20, "gg", IDate);
//                    table.Rows.Add(21, "hh", IDate);
//                    table.Rows.Add(22, "ii", IDate);
//                    table.Rows.Add(23, "kk", IDate);
//                    table.Rows.Add(24, "ll", IDate);
//                    table.Rows.Add(25, "mm", IDate);
//                    table.Rows.Add(26, "uu", IDate);
//                    table.Rows.Add(27, "tt", IDate);
//                    table.Rows.Add(32, "oo", IDate);
//                    table.Rows.Add(33, "pp", IDate);
//                    table.Rows.Add(34, "qq", IDate);
//                    table.Rows.Add(35, "ww", IDate);
//                    table.Rows.Add(36, "zz", IDate);
//                }
//                if (i == 2)
//                {
//                    table.Rows.Add(3, "c", IDate);
//                    table.Rows.Add(4, "d", IDate);
//                    table.Rows.Add(5, "e", IDate);
//                    table.Rows.Add(6, "f", IDate);
//                    table.Rows.Add(7, "g", IDate);
//                    table.Rows.Add(11, "aa", IDate);
//                    table.Rows.Add(12, "bb", IDate);
//                    table.Rows.Add(13, "cc", IDate);
//                    table.Rows.Add(14, "dd", IDate);
//                    table.Rows.Add(15, "ee", IDate);
//                    table.Rows.Add(16, "ff", IDate);
//                    table.Rows.Add(20, "gg", IDate);
//                    table.Rows.Add(21, "hh", IDate);
//                    table.Rows.Add(22, "ii", IDate);
//                    table.Rows.Add(23, "kk", IDate);
//                    table.Rows.Add(24, "ll", IDate);
//                    table.Rows.Add(25, "mm", IDate);
//                    table.Rows.Add(26, "uu", IDate);
//                    table.Rows.Add(27, "tt", IDate);
//                    table.Rows.Add(32, "oo", IDate);
//                    table.Rows.Add(33, "pp", IDate);
//                    table.Rows.Add(34, "qq", IDate);
//                    table.Rows.Add(35, "ww", IDate);
//                    table.Rows.Add(36, "zz", IDate);
//                }
//                if (i == 3)
//                {
//                    table.Rows.Add(5, "e", IDate);
//                    table.Rows.Add(6, "f", IDate);
//                    table.Rows.Add(7, "g", IDate);
//                    table.Rows.Add(11, "aa", IDate);
//                    table.Rows.Add(12, "bb", IDate);
//                    table.Rows.Add(13, "cc", IDate);
//                    table.Rows.Add(14, "dd", IDate);
//                    table.Rows.Add(15, "ee", IDate);
//                    table.Rows.Add(16, "ff", IDate);
//                    table.Rows.Add(20, "gg", IDate);
//                    table.Rows.Add(21, "hh", IDate);
//                    table.Rows.Add(22, "ii", IDate);
//                    table.Rows.Add(23, "kk", IDate);
//                    table.Rows.Add(24, "ll", IDate);
//                    table.Rows.Add(25, "mm", IDate);
//                    table.Rows.Add(26, "uu", IDate);
//                    table.Rows.Add(27, "tt", IDate);
//                    table.Rows.Add(32, "oo", IDate);
//                    table.Rows.Add(33, "pp", IDate);
//                    table.Rows.Add(34, "qq", IDate);
//                    table.Rows.Add(35, "ww", IDate);
//                    table.Rows.Add(36, "zz", IDate);
//                }
//                if (i == 4)
//                {
//                    table.Rows.Add(7, "g", IDate);
//                    table.Rows.Add(11, "aa", IDate);
//                    table.Rows.Add(12, "bb", IDate);
//                    table.Rows.Add(13, "cc", IDate);
//                    table.Rows.Add(14, "dd", IDate);
//                    table.Rows.Add(15, "ee", IDate);
//                    table.Rows.Add(16, "ff", IDate);
//                    table.Rows.Add(20, "gg", IDate);
//                    table.Rows.Add(21, "hh", IDate);
//                    table.Rows.Add(22, "ii", IDate);
//                    table.Rows.Add(23, "kk", IDate);
//                    table.Rows.Add(24, "ll", IDate);
//                    table.Rows.Add(25, "mm", IDate);
//                    table.Rows.Add(26, "uu", IDate);
//                    table.Rows.Add(27, "tt", IDate);
//                    table.Rows.Add(32, "oo", IDate);
//                    table.Rows.Add(33, "pp", IDate);
//                    table.Rows.Add(34, "qq", IDate);
//                    table.Rows.Add(35, "ww", IDate);
//                    table.Rows.Add(36, "zz", IDate);
//                }
//                if (i == 5)
//                {
//                    table.Rows.Add(12, "bb", IDate);
//                    table.Rows.Add(13, "cc", IDate);
//                    table.Rows.Add(14, "dd", IDate);
//                    table.Rows.Add(15, "ee", IDate);
//                    table.Rows.Add(16, "ff", IDate);
//                    table.Rows.Add(20, "gg", IDate);
//                    table.Rows.Add(21, "hh", IDate);
//                    table.Rows.Add(22, "ii", IDate);
//                    table.Rows.Add(23, "kk", IDate);
//                    table.Rows.Add(24, "ll", IDate);
//                    table.Rows.Add(25, "mm", IDate);
//                    table.Rows.Add(26, "uu", IDate);
//                    table.Rows.Add(27, "tt", IDate);
//                    table.Rows.Add(32, "oo", IDate);
//                    table.Rows.Add(33, "pp", IDate);
//                    table.Rows.Add(34, "qq", IDate);
//                    table.Rows.Add(35, "ww", IDate);
//                    table.Rows.Add(36, "zz", IDate);
//                    //
//                }
//                if (i == 6)
//                {
//                    table.Rows.Add(14, "dd", IDate);
//                    table.Rows.Add(15, "ee", IDate);
//                    table.Rows.Add(16, "ff", IDate);
//                    table.Rows.Add(20, "gg", IDate);
//                    table.Rows.Add(21, "hh", IDate);
//                    table.Rows.Add(22, "ii", IDate);
//                    table.Rows.Add(23, "kk", IDate);
//                    table.Rows.Add(24, "ll", IDate);
//                    table.Rows.Add(25, "mm", IDate);
//                    table.Rows.Add(26, "uu", IDate);
//                    table.Rows.Add(27, "tt", IDate);
//                    table.Rows.Add(32, "oo", IDate);
//                    table.Rows.Add(33, "pp", IDate);
//                    table.Rows.Add(34, "qq", IDate);
//                    table.Rows.Add(35, "ww", IDate);
//                    table.Rows.Add(36, "zz", IDate);
//                    //
//                }
//                if (i == 7)
//                {
//                    table.Rows.Add(16, "ff", IDate);
//                    table.Rows.Add(20, "gg", IDate);
//                    table.Rows.Add(21, "hh", IDate);
//                    table.Rows.Add(22, "ii", IDate);
//                    table.Rows.Add(23, "kk", IDate);
//                    table.Rows.Add(24, "ll", IDate);
//                    table.Rows.Add(25, "mm", IDate);
//                    table.Rows.Add(26, "uu", IDate);
//                    table.Rows.Add(27, "tt", IDate);
//                    table.Rows.Add(32, "oo", IDate);
//                    table.Rows.Add(33, "pp", IDate);
//                    table.Rows.Add(34, "qq", IDate);
//                    table.Rows.Add(35, "ww", IDate);
//                    table.Rows.Add(36, "zz", IDate);
//                    //
//                }
//                if (i == 8)
//                {
//                    table.Rows.Add(21, "hh", IDate);
//                    table.Rows.Add(22, "ii", IDate);
//                    table.Rows.Add(23, "kk", IDate);
//                    table.Rows.Add(24, "ll", IDate);
//                    table.Rows.Add(25, "mm", IDate);
//                    table.Rows.Add(26, "uu", IDate);
//                    table.Rows.Add(27, "tt", IDate);
//                    table.Rows.Add(32, "oo", IDate);
//                    table.Rows.Add(33, "pp", IDate);
//                    table.Rows.Add(34, "qq", IDate);
//                    table.Rows.Add(35, "ww", IDate);
//                    table.Rows.Add(36, "zz", IDate);

//                    //
//                }
//                if (i == 9)
//                {
//                    table.Rows.Add(23, "kk", IDate);
//                    table.Rows.Add(24, "ll", IDate);
//                    table.Rows.Add(25, "mm", IDate);
//                    table.Rows.Add(26, "uu", IDate);
//                    table.Rows.Add(27, "tt", IDate);
//                    table.Rows.Add(32, "oo", IDate);
//                    table.Rows.Add(33, "pp", IDate);
//                    table.Rows.Add(34, "qq", IDate);
//                    table.Rows.Add(35, "ww", IDate);
//                    table.Rows.Add(36, "zz", IDate);
//                    //

//                }
//                if (i == 10)
//                {
//                    table.Rows.Add(25, "mm", IDate);
//                    table.Rows.Add(26, "uu", IDate);
//                    table.Rows.Add(27, "tt", IDate);
//                    table.Rows.Add(32, "oo", IDate);
//                    table.Rows.Add(33, "pp", IDate);
//                    table.Rows.Add(34, "qq", IDate);
//                    table.Rows.Add(35, "ww", IDate);
//                    table.Rows.Add(36, "zz", IDate);
//                }
//                if (i == 11)
//                {
//                    table.Rows.Add(27, "tt", IDate);
//                    table.Rows.Add(32, "oo", IDate);
//                    table.Rows.Add(33, "pp", IDate);
//                    table.Rows.Add(34, "qq", IDate);
//                    table.Rows.Add(35, "ww", IDate);
//                    table.Rows.Add(36, "zz", IDate);
//                }
//                if (i == 12)
//                {
//                    table.Rows.Add(33, "pp", IDate);
//                    table.Rows.Add(34, "qq", IDate);
//                    table.Rows.Add(35, "ww", IDate);
//                    table.Rows.Add(36, "zz", IDate);
//                }
//                if (i == 13)
//                {
//                    table.Rows.Add(35, "ww", IDate);
//                    table.Rows.Add(36, "zz", IDate);

//                }
//                if (i == 14)
//                {
//                    //should be zero             
//                }
            

//            }

//            this.SetDataTable(table);
        
//        }

//       public DataTable InitDataTable(){

//            this.UserSessionMeta = new DataTable();
//            if (this.UserSessionMeta.Rows.Count <= 0)
//            {
//                this.UserSessionMeta.Clear();
//                this.UserSessionMeta.Columns.Add("ID", typeof(int));
//                this.UserSessionMeta.Columns.Add("UserId", typeof(string));
//                this.UserSessionMeta.Columns.Add("RecordTimestamp", typeof(string));

//            }
//            return this.UserSessionMeta;
//        }

//       public DataTable GetDataTable() {
//           return this.UserSessionMeta;
//       }

//       public void SetDataTable(DataTable dt) {
//           this.UserSessionMeta = dt;
//       }
//    }   
//    }

//    public class TwoWeekRetention {

//        public DataTable Retention {get;set;}


//        public DataTable GetTable(){
//            return this.Retention;
//        }

//        public void InsertRow(RetentionRow row){

//            DataTable RetentionView = this.GetTable();
//            int index = 3;

//            object[] array = new object[17];
//            array[0] = 00;
//            array[1] = row.date;
//            array[2] = row.installsOnThisDay;

//            foreach(float percent in row.days){
//                array[index] = percent;
//                index++;
//            }

//            RetentionView.Rows.Add(array);
        
//        }
//        public void InsertRow(List<RetentionRow> rows){
//            DataTable RetentionView = this.GetTable();
//            foreach(RetentionRow row in rows){                
//                int index = 3;
//                object[] array = new object[17];
//                array[0] = 00;
//                array[1] = row.date;
//                array[2] = row.installsOnThisDay;

//                foreach(float percent in row.days){
//                    array[index] = percent;
//                    index++;
//                }

//                RetentionView.Rows.Add(array);            
//            }
        
//        }
//        public DataTable InitDataTable()
//        {

//            DataTable TwoWeekView = this.Retention;

//            if (TwoWeekView != null)
//            {
//                if (TwoWeekView.Rows.Count > 0)
//                {
//                    TwoWeekView.Clear();
//                    TwoWeekView.Columns.Add("ID", typeof(int));
//                    TwoWeekView.Columns.Add("NewUsers", typeof(int));
//                    TwoWeekView.Columns.Add("Date", typeof(DateTime));


//                    for (int i = 1; i < 15; i++)
//                    {
//                        TwoWeekView.Columns.Add(String.Format("Day{0}", i), typeof(float));
//                    }

                    

//                }
//            }

//            return TwoWeekView;
//        }  
//    }

//    [TestFixture]
//    public class RetentionTests
//    {
//        public static MockFirstLoginTable FirstLoginTable = new MockFirstLoginTable();
//        public static MockLoginTable LoginTable = new MockLoginTable();


//        [TestFixtureSetUp]
//        public void HarnessSetup()
//        {
            
            

//        }

//        [Test]
//        public void TestDayNPercents()
//        {
//            MockRetentionRow row = new MockRetentionRow();
//            row.date = DateTime.UtcNow.AddDays(-14);
//            row.installsOnThisDay = FirstLoginTable.GetNewUsersCount(row.date);
//            Retention.Instance.UpdateDayPercents(row);
//            //foreach (float f in row.days) {
//            //    Logger.Instance.Info(f);
//            //}
//            for (int i = 14; i >= 1; i--)
//            {
//                Logger.Instance.Info(String.Format("-= Day {2}: Expected 100%: ({0} / {1}) * 100 =-", 100, 100, i));
//                Logger.Instance.Info(FirstLoginTable.GetNewUsersCount(DateTime.UtcNow.AddDays(-i)).ToString());
//                Assert.AreEqual(26, FirstLoginTable.GetNewUsersCount(DateTime.UtcNow.AddDays(-i)).ToString());
//            }
            

//        }
//        [Test]
//        public void TestMockNewUserTable()
//        {
//            Logger.Instance.Info("-= Testing that we have a Mock FirstLoginTable =-");
//            Assert.AreEqual(26, FirstLoginTable.GetNewUsersCount(DateTime.UtcNow.AddDays(-14)));

//        }
//        [Test]
//        public void TestMockLoginTable()
//        {
//            Logger.Instance.Info("-= Testing that we have a Mock Login Table =-");
//            Assert.AreEqual(26, FirstLoginTable.GetNewUsersCount(DateTime.UtcNow.AddDays(-14)));

//        }
//    }
