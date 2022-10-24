using System;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace NoreSources.Build
{
	public class ConsoleLogger : Logger
	{
		public override void Initialize(IEventSource eventSource)
		{
			Console.Out.WriteLine("Initialize " + Verbosity);
			eventSource.ProjectStarted += new ProjectStartedEventHandler(
			eventSource_ProjectStarted);
			eventSource.WarningRaised += new BuildWarningEventHandler(
					eventSource_WarningRaised);
			eventSource.ErrorRaised += new BuildErrorEventHandler(
				eventSource_ErrorRaised);
		}

		void eventSource_ErrorRaised(
					object sender,
					 BuildErrorEventArgs e)
		{
			string file = GetRelativePath(e.File);
			string line = String.Format("{0}({1},{2}) {3}: {4}",
				file, e.LineNumber, e.ColumnNumber, "error", e.Message);
			Console.Error.WriteLine(line);
		}

		void eventSource_WarningRaised(object sender, BuildWarningEventArgs e)
		{
			string file = GetRelativePath(e.File);
			string line = String.Format("{0}({1},{2}) {3}: {4}",
				file, e.LineNumber, e.ColumnNumber, "warning", e.Message);
			Console.Error.WriteLine(line);
		}

		void eventSource_MessageRaised(
					object sender,
					 BuildMessageEventArgs e)
		{

		}

		void eventSource_TaskStarted(
					object sender,
					 TaskStartedEventArgs e)
		{
		}

		void eventSource_ProjectStarted(
					object sender,
					 ProjectStartedEventArgs e)
		{
			//Console.Out.WriteLine ("Starting " + e.ProjectFile);
		}

		void eventSource_ProjectFinished(
					object sender,
					 ProjectFinishedEventArgs e)
		{
		}

		static string GetRelativePath(string file)
		{
			/*
				file = Path.GetRelativePath(
					Environment.CurrentDirectory.ToString(),
					file);
				*/
			Uri f = new Uri(file);
			Uri c = new Uri(Path.Combine(Environment.CurrentDirectory, "dumm y"));

			return c.MakeRelativeUri(f).ToString();
		}

	}
}