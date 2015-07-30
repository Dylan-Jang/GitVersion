﻿using System;
using System.Collections.Generic;
using System.IO;
using GitVersion;
using NUnit.Framework;
using Shouldly;

[TestFixture]
public class JenkinsMessageGenerationTests
{
    [Test]
    public void GenerateSetVersionMessageReturnsVersionAsIs_AlthoughThisIsNotUsedByJenkins()
    {
        var j = new Jenkins();
        var v = j.GenerateSetVersionMessage("0.0.0-Beta4.7");
        Assert.AreEqual                    ("0.0.0-Beta4.7", v);
    }

    [Test]
    public void GenerateMessageTest()
    {
        var j = new Jenkins();
        var actual = j.GenerateSetParameterMessage("name", "value");
        Assert.AreEqual(1, actual.Length);
        Assert.AreEqual("GitVersion_name=value", actual[0]);
    }

    [Test, Explicit]
    public void WriteAllVariablesToTheTextWriter()
    {
        // this test method writes to disc, hence marked explicit
        var f = "this_file_should_be_deleted.properties";

        try
        {
            AssertVariablesAreWrittenToFile(f);
        }
        finally
        {
            File.Delete(f);
        }
    }

    static void AssertVariablesAreWrittenToFile(string f)
    {
        var writes = new List<string>();
        var semanticVersion = new SemanticVersion
            {
                Major = 1,
                Minor = 2,
                Patch = 3,
                PreReleaseTag = "beta1",
                BuildMetaData = "5"
            };

        semanticVersion.BuildMetaData.CommitDate = DateTimeOffset.Parse("2014-03-06 23:59:59Z");
        semanticVersion.BuildMetaData.Sha = "commitSha";
        var variables = VariableProvider.GetVariablesFor(semanticVersion, AssemblyVersioningScheme.MajorMinorPatch, VersioningMode.ContinuousDelivery, "ci", false);

        var j = new Jenkins(f);

        j.WriteIntegration(writes.Add, variables);

        writes[1].ShouldBe("1.2.3-beta.1+5");

        Assert.True(File.Exists(f));

        var props = File.ReadAllText(f);

        StringAssert.Contains("GitVersion_Major=1", props);
        StringAssert.Contains("GitVersion_Minor=2", props);
    }
}