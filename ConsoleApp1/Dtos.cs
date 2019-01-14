using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagazineStoreApp
{

    public class Data
    {
        public string totalTime { get; set; }
        public bool answerCorrect { get; set; }
        public List<string> shouldBe { get; set; }
    }

    public class Response
    {
        public Data data { get; set; }
        public bool success { get; set; }
        public string token { get; set; }
        public string message { get; set; }
    }

    public class Answer
    {
        public List<string> subscribers { get; set; }
    }

}
