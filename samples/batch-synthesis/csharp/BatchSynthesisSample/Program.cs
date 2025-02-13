//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
//

// Your Speech resource key and region
// This example requires environment variables named "SPEECH_KEY" and "SPEECH_REGION"
string speechRegion = Environment.GetEnvironmentVariable("SPEECH_REGION") 
    ?? throw new ArgumentException("Please set SPEECH_REGION environment variable.");

string speechKey = Environment.GetEnvironmentVariable("SPEECH_KEY") 
    ?? throw new ArgumentException("Please set the SPEECH_KEY environment variable to set speech resource key.");

string apiVersion = "2024-04-01";
var host = $"https://{speechRegion}.api.cognitive.microsoft.com";
var sampleScript = await File.ReadAllTextAsync("../../Gatsby-chapter1.txt").ConfigureAwait(false);

var synthesisClient = new BatchSynthesisClient(host, speechKey, apiVersion);

// Get all synthesis jobs.
var synthesisJobs = await synthesisClient.GetAllSynthesesAsync().ConfigureAwait(false);
Console.WriteLine($"Found {synthesisJobs.Count()} jobs.");

var newJobId = $"SimpleJob-{DateTime.Now:u}".Replace(":", "-").Replace(" ", "-");

// Create a new synthesis job with plain text
await synthesisClient.CreateSynthesisAsync(newJobId, "en-US-AvaMultilingualNeural", sampleScript, false).ConfigureAwait(false);

// Get a synthesis job.
var synthesis = await synthesisClient.GetSynthesisAsync(newJobId).ConfigureAwait(false);

// Poll the synthesis until it completes
var terminatedStates = new[] { "Succeeded", "Failed" };
while (!terminatedStates.Contains(synthesis.Status))
{
    Console.WriteLine($"Synthesis {newJobId}. Status: {synthesis.Status}");
    await Task.Delay(TimeSpan.FromSeconds(30)).ConfigureAwait(false);
    synthesis = await synthesisClient.GetSynthesisAsync(newJobId).ConfigureAwait(false);
}

Console.WriteLine($"Synthesis {newJobId}. Status: {synthesis.Status}");

// Get outputs of the synthesis
if (!string.IsNullOrEmpty(synthesis.Outputs?.Result))
{
    Console.WriteLine("Please download result from this URL before you delete the synthesis.");
    Console.WriteLine(synthesis.Outputs.Result);
}

if (!string.IsNullOrEmpty(synthesis.Outputs?.Summary))
{
    Console.WriteLine("Please download summary file from this URL before you delete the synthesis.");
    Console.WriteLine(synthesis.Outputs.Summary);
}

// Ask for confirmation before deleting the synthesis
Console.Write("Do you want to delete the synthesis job? (y/n): ");
var input = Console.ReadLine();
if (input?.Trim().ToLower() == "y")
{
    await synthesisClient.DeleteSynthesisAsync(newJobId);
    Console.WriteLine($"Deleted synthesis {newJobId}.");
}
else
{
    Console.WriteLine("Synthesis deletion cancelled.");
}