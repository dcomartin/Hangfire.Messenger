var target = Argument("target", "MSBuild");

Task("MSBuild")
	.Does(() =>
	{
	  MSBuild("Hangfire.Messenger.sln");
	});

RunTarget(target);