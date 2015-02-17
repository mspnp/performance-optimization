namespace BackgroundProcessor.Logic.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class GameLogicTest
    {
        [TestMethod]
        public void When_A_Letter_In_User_Word_Matches_But_Not_By_Position_Gives_One_Cow()
        {
            const string GameWord = "Kiln";
            const string UserWord = "Rack";

            Assert.AreEqual("1", WordAnalyzer.AnalyseUserInput(UserWord, GameWord));
        }

        [TestMethod]
        public void When_A_Letter_In_User_Word_Matches_But_Not_By_Position_Gives_One_Cow_With_Array()
        {
            var gameWords = new[]{ "Kiln", "Acks", "Boxy", "Foxy" };
            var userWords = new[] { "Rack", "Krip", "Abet", "Opte" };

            for (var i = 0; i < gameWords.Length; i++)
            {
                Assert.AreEqual("1", WordAnalyzer.AnalyseUserInput(userWords[i], gameWords[i]));
            }
        }

        [TestMethod]
        public void When_A_Letter_In_User_Word_Matches_And_In_Exact_Position_Gives_One_Bull()
        {
            const string GameWord = "Kiln";
            const string UserWord = "Kram";

            Assert.AreEqual("0", WordAnalyzer.AnalyseUserInput(UserWord, GameWord));
        }

        [TestMethod]
        public void When_No_Letter_In_User_Word_Matches_Gives_Nil()
        {
            const string GameWord = "Kiln";
            const string UserWord = "Ramp";

            Assert.AreEqual(string.Empty, WordAnalyzer.AnalyseUserInput(UserWord, GameWord));
        }

        [TestMethod]
        public void When_Two_Letters_In_User_Word_Match_And_In_Exact_Position_Gives_Two_Bull()
        {
            const string GameWord = "Kiln";
            const string UserWord = "Kite";

            Assert.AreEqual("00", WordAnalyzer.AnalyseUserInput(UserWord, GameWord));
        }

        [TestMethod]
        public void When_Two_Letters_In_User_Word_Match_But_Not_By_Position_Gives_Two_Cow()
        {
            const string GameWord = "Kiln";
            const string UserWord = "Lone";

            Assert.AreEqual("11", WordAnalyzer.AnalyseUserInput(UserWord, GameWord));
        }

        [TestMethod]
        public void When_Three_Letters_In_User_Word_Match_And_In_Exact_Position_Gives_Three_Bull()
        {
            const string GameWord = "Kiln";
            const string UserWord = "Kilt";

            Assert.AreEqual("000", WordAnalyzer.AnalyseUserInput(UserWord, GameWord));
        }

        [TestMethod]
        public void When_Three_Letters_In_User_Word_Match_But_Not_By_Position_Gives_Three_Cow()
        {
            const string GameWord = "Kiln";
            const string UserWord = "Lonk";

            Assert.AreEqual("111", WordAnalyzer.AnalyseUserInput(UserWord, GameWord));
        }

        [TestMethod]
        public void When_Four_Letters_In_User_Word_Match_And_In_Exact_Position_Gives_Four_Bull()
        {
            const string GameWord = "Kiln";
            const string UserWord = "Kiln";

            Assert.AreEqual("0000", WordAnalyzer.AnalyseUserInput(UserWord, GameWord));
        }

        [TestMethod]
        public void When_Four_Letters_In_User_Word_Match_But_Not_By_Position_Gives_Four_Cow()
        {
            const string GameWord = "Kiln";
            const string UserWord = "Ilnk";

            Assert.AreEqual("1111", WordAnalyzer.AnalyseUserInput(UserWord, GameWord));
        }

        [TestMethod]
        public void When_Two_Letters_In_User_Word_Match_And_One_In_Pos_Second_Out_Of_Pos_Gives_One_Bull_One_Cow()
        {
            const string GameWord = "Kiln";
            const string UserWord = "Kant";

            Assert.AreEqual("10", WordAnalyzer.AnalyseUserInput(UserWord, GameWord));
        }

        [TestMethod]
        public void When_Three_Letters_In_User_Word_Match_And_One_In_Pos_Other_Two_Out_Of_Pos_Gives_One_Bull_Two_Cow()
        {
            const string GameWord = "Kiln";
            const string UserWord = "Kail";

            Assert.AreEqual("110", WordAnalyzer.AnalyseUserInput(UserWord, GameWord));
        }

        [TestMethod]
        public void When_Three_Letters_In_User_Word_Match_And_Two_In_Pos_Other_One_Out_Of_Pos_Gives_Two_Bull_One_Cow()
        {
            const string GameWord = "Kiln";
            const string UserWord = "Kial";

            Assert.AreEqual("100", WordAnalyzer.AnalyseUserInput(UserWord, GameWord));
        }

        [TestMethod]
        public void When_Four_Letters_In_User_Word_Match_And_Two_In_Pos_Other_Two_Out_Of_Pos_Gives_Two_Bull_Two_Cow()
        {
            const string GameWord = "Kiln";
            const string UserWord = "Kinl";

            Assert.AreEqual("1100", WordAnalyzer.AnalyseUserInput(UserWord, GameWord));
        }
    }
}
