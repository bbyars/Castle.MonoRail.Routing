using System.Collections;
using System.IO;
using System.Threading;
using Castle.MonoRail.Routing;
using NUnit.Framework;

namespace Castle.MonoRail.Routing.Test
{
    [TestFixture]
    public class AssetHelperTest
    {
        private AssetHelper helper;

        [SetUp]
        public void SetUp()
        {
            helper = new AssetHelper { MissingImage = "missing.jpg" };
            helper.ImagesRoot = helper.CssRoot = helper.JavaScriptRoot = "assets";
            Directory.CreateDirectory("assets");
        }

        [TearDown]
        public void TearDown()
        {
            Directory.Delete("assets", true);
        }

        [Test]
        public void ImageTimestampAdded()
        {
            File.WriteAllText("assets/folder.png", "");
            var expected = string.Format("<img src=\"assets/folder.png?{0}\" />", GetAssetId("assets/folder.png"));
            Assert.AreEqual(expected, helper.Image("folder.png"));
        }

        [Test]
        public void ImageWithExtraAttributes()
        {
            File.WriteAllText("assets/folder.png", "");
            IDictionary attributes = new Hashtable { { "class", "test" } };
            var expected = string.Format("<img src=\"assets/folder.png?{0}\" class=\"test\" />",
                GetAssetId("assets/folder.png"));
            Assert.AreEqual(expected, helper.Image("folder.png", attributes));
        }

        [Test]
        public void ImageForMissingFile()
        {
            Assert.AreEqual("<img src=\"assets/missing.jpg\" />", helper.Image("wrong.jpg"));
        }

        [Test]
        public void CssWithTimestamp()
        {
            File.WriteAllText("assets/test.css", "");
            var expected = string.Format("<link href=\"assets/test.css?{0}\" rel=\"Stylesheet\" type=\"text/css\" />",
                GetAssetId("assets/test.css"));
            Assert.AreEqual(expected, helper.Css("test.css"));
        }

        [Test]
        public void MissingCss()
        {
            var expected = "<link href=\"assets/missing.css\" rel=\"Stylesheet\" type=\"text/css\" />";
            Assert.AreEqual(expected, helper.Css("missing.css"));
        }

        [Test]
        public void JavaScriptWithTimestamp()
        {
            File.WriteAllText("assets/test.js", "");
            var expected = string.Format("<script src=\"assets/test.js?{0}\" type=\"text/javascript\"></script>",
                GetAssetId("assets/test.js"));
            Assert.AreEqual(expected, helper.JavaScript("test.js"));
        }

        [Test]
        public void WithQuerystring()
        {
            File.WriteAllText("assets/test.js", "");
            var expected = string.Format("<script src=\"assets/test.js?{0}&param=help\" type=\"text/javascript\"></script>",
                GetAssetId("assets/test.js"));
            Assert.AreEqual(expected, helper.JavaScript("test.js?param=help"));
        }

        [Test]
        public void QuerystringChangesIfFileChanges()
        {
            File.WriteAllText("assets/folder.png", "");
            var originalUrl = helper.Image("folder.png");
            Thread.Sleep(100);
            File.WriteAllText("assets/folder.png", "new content");
            Assert.AreNotEqual(originalUrl, helper.Image("folder.png"));
        }

        private string GetAssetId(string filename)
        {
            var lastEditTime = File.GetLastWriteTime(filename);
            return lastEditTime.Ticks.ToString();
        }
    }
}