using System;

namespace data_old.code {
  public partial class Index : System.Web.UI.Page {
    protected void Page_Load(object sender, EventArgs e) {
      xmlToTsx.convert.toTsxDir(@"d:\rw\data-src\lm\oldea", @"d:\rw\data\lm\oldea");
      //xmlToTsx.convert.toTsxDir(@"D:\rw\rw\rw-course\examples", @"D:\rw\rw\rw-course\examples");
    }
  }
}