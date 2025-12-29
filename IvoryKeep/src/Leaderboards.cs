using Rainfall;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml;
using HtmlAgilityPack;


public struct LeaderboardEntry
{
	public string name;
	public int score;
	public float time;
}

public static class Leaderboards
{
	const string url = "DHK9cC8tiJ6Lx1wBv_pOSd14GxMkiZC8jAvnkbA2Jd8";
	const string speedrunUrl = "IFwcyVt6KlAe6vts-yqc-lEk2Z4O9eHKmRdIqDuKUj0";


	public static List<LeaderboardEntry> leaderboard = new List<LeaderboardEntry>();


	public static void OnRunFinishedScore(RunStats run, SaveFile save)
	{
		_ = SendLeaderboardRun(save.name, url, run.score.ToString()); // Score leaderboard
	}

	public static void OnRunFinishedTime(RunStats run, SaveFile save)
	{
		_ = SendLeaderboardRun(save.name, speedrunUrl, (run.duration / 60).ToString()); // Speedrun leaderboard
	}

	async static Task SendLeaderboardRun(string name, string leaderboardToken, string score)
	{
		var client = new HttpClient();

		JsonObject value = new()
			{
				{ "player_username", name },
				{ "score", score }
			};

		string json = value.ToJsonString();

		client.DefaultRequestHeaders.Add("x-access-token", leaderboardToken);
		client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

		var builder = new UriBuilder(new Uri("https://leaderboards.dev/score"));

		HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, builder.Uri);
		request.Content = new StringContent(json, Encoding.UTF8, "application/json");

		HttpResponseMessage response = await client.SendAsync(request);
		string result = await response.Content.ReadAsStringAsync();
		Console.WriteLine(result);
	}

	public static async Task FetchLeaderboardData()
	{
		string url = "https://leaderboards.dev/user/Scewps/IvoryKeep";

		using var http = new HttpClient();
		var html = await http.GetStringAsync(url);

		var doc = new HtmlDocument();
		doc.LoadHtml(html);

		// Select rows inside the leaderboard table
		var rows = doc.DocumentNode.SelectNodes(
			"//table[@id='example']/tbody/tr"
		);

		if (rows == null)
			return;

		leaderboard.Clear();

		foreach (var row in rows)
		{
			var cells = row.SelectNodes("td");
			if (cells == null || cells.Count < 3)
				continue;

			string rank = cells[0].InnerText.Trim();
			string score = cells[1].InnerText.Trim();
			string username = cells[2].InnerText.Trim();

			LeaderboardEntry entry = new LeaderboardEntry();
			entry.name = username;
			entry.score = (int)float.Parse(score);
			entry.time = -1;
			leaderboard.Add(entry);
		}
	}
}
