using System;
using TemplateParser2022.Test;

// You can run this if you can't get the tests to run.

TemplateEngineTests test = new TemplateEngineTests();
test.Test_basic_local_property_substitute();
test.Test_scoped_property_substitute();
test.Test_spanned_local_property_substitute();
test.Test_invalid_property_substitute();
test.Test_no_property_substitute();

Console.WriteLine("All tests passed successfully!");
Console.ReadKey();

return 0;
