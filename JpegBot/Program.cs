using System.Text.RegularExpressions;
using DSharpPlus;
using DSharpPlus.Entities;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;

var discord = new DiscordClient(new DiscordConfiguration() {
	Token = Environment.GetEnvironmentVariable("BOT_TOKEN"),
	Intents = DiscordIntents.GuildMessages
});

var http = new HttpClient();

string? GetImageUrl(DiscordMessage message) {
	return message.Attachments.FirstOrDefault(att => att.MediaType.StartsWith("image/"))?.Url ?? message.Embeds.FirstOrDefault(embed => embed.Image != null)?.Image.Url.ToString();
}

var regex = new Regex("needs? ?more ?jpe?g", RegexOptions.IgnoreCase);

discord.MessageCreated += (dc, args) => {
	if (!args.Author.IsBot && regex.IsMatch(args.Message.Content)) {
		_ = Task.Run(async () => {
			string? attachmentUrl =
				GetImageUrl(args.Message) ??
				GetImageUrl(args.Message.ReferencedMessage) ??
				(await args.Channel.GetMessagesBeforeAsync(args.Message.Id - 1, 5)).OrderByDescending(message => message.CreationTimestamp).Select(GetImageUrl).FirstOrDefault(url => url != null);
			
			if (attachmentUrl == null) {
				return;
			}
			
			Image image;
			await using (Stream download = await http.GetStreamAsync(attachmentUrl)) {
				image = await Image.LoadAsync(download);
			}
			
			image.Mutate(ctx => ctx.Resize(ctx.GetCurrentSize() * 3 / 4, new NearestNeighborResampler(), true));

			await using (var ms = new MemoryStream()) {
				var jpegEncoder = new JpegEncoder() {
					Quality = image.Metadata.GetJpegMetadata().Quality / 8
				};
				await image.SaveAsJpegAsync(ms, jpegEncoder);
				ms.Seek(0, SeekOrigin.Begin);
				await args.Message.RespondAsync(dmb => dmb.WithFile("more.jpg", ms));
			}
		});
	}
	return Task.CompletedTask;
};

await discord.ConnectAsync();

await Task.Delay(-1);
