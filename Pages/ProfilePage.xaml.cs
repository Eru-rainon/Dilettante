using Dilettante.Data;
using Dilettante.Models;
using Dilettante.Views;
using Microsoft.EntityFrameworkCore;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;

namespace Dilettante.Pages
{
    public partial class ProfilePage : Page
    {
        public ProfilePage()
        {
            InitializeComponent();
            Loaded += OnPageLoaded;
        }

        private void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            using var db = new AppDbContext();
            var allGames = db.Games.Include(g => g.Achievements).ToList();
            var playedGames = allGames.Where(g => g.Status != GameStatus.Wishlisted).ToList();

            // Library counts
            TotalGamesText.Text = allGames.Count.ToString();
            PlayingText.Text = allGames.Count(g => g.Status == GameStatus.Playing).ToString();
            CompletedText.Text = allGames.Count(g => g.Status == GameStatus.Completed).ToString();
            WishlistedText.Text = allGames.Count(g => g.Status == GameStatus.Wishlisted).ToString();
            PausedText.Text = allGames.Count(g => g.Status == GameStatus.Paused).ToString();
            AbandonedText.Text = allGames.Count(g => g.Status == GameStatus.Abandoned).ToString();

            int totalPlayed = playedGames.Count;
            LibraryFlavourText.Text = totalPlayed switch
            {
                0 => "Your library is empty. The journey hasn't begun.",
                <= 5 => "Just getting started. The backlog grows.",
                <= 15 => "A respectable collection taking shape.",
                <= 30 => "The backlog is becoming a lifestyle.",
                <= 50 => "At this point, retirement might be needed to finish these.",
                _ => "You have a problem. A beautiful, beautiful problem."
            };

            // Achievement stats
            var gamesWithAchievements = allGames.Where(g => g.Achievements?.Count > 0).ToList();
            GamesTrackedText.Text = gamesWithAchievements.Count.ToString();

            
            int unlockedAchievements = gamesWithAchievements.Sum(g => g.Achievements!.Count(a => a.IsUnlocked));
            TotalAchievementsText.Text = unlockedAchievements.ToString();

            if (gamesWithAchievements.Count > 0)
            {
                double avgCompletion = gamesWithAchievements
                    .Average(g => (double)g.Achievements!.Count(a => a.IsUnlocked) / g.Achievements.Count * 100);
                AvgCompletionText.Text = $"{avgCompletion:F1}%";

                AchievementFlavourText.Text = avgCompletion switch
                {
                    >= 90 => "A completionist of the highest order.",
                    >= 70 => "Thorough. Methodical. Slightly obsessive.",
                    >= 50 => "You finish what you start. Mostly.",
                    >= 25 => "The easy ones only. We don't judge.",
                    _ => "Achievements are just suggestions, really."
                };
            }
            else
            {
                AvgCompletionText.Text = "N/A";
                AchievementFlavourText.Text = "No achievements tracked yet.";
            }

            // Integrity score
            if (playedGames.Count > 0)
            {
                int legitimate = playedGames.Count(g =>
                    g.Ownership == GameOwnership.OwnedCopy ||
                    g.Ownership == GameOwnership.Subscription);
                int seafared = playedGames.Count(g => g.Ownership == GameOwnership.Seafared);
                double integrityScore = (double)legitimate / playedGames.Count * 10;

                IntegrityScoreText.Text = $"{integrityScore:F1}";
                LegitimateText.Text = legitimate.ToString();
                SeafaredText.Text = seafared.ToString();

                IntegrityFlavourText.Text = integrityScore switch
                {
                    >= 9 => "Absolutely squeaky clean.",
                    >= 7 => "Mostly legitimate.",
                    >= 5 => "A complicated relationship with ownership.",
                    >= 3 => "The seas call to you.",
                    _ => "A true pirate of the high seas."
                };
            }
            else
            {
                IntegrityScoreText.Text = "N/A";
                LegitimateText.Text = "0";
                SeafaredText.Text = "0";
                IntegrityFlavourText.Text = "";
            }
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog {
                FileName = $"Dilettante_Collection_{DateTime.Now:yyyy-MM-dd}",
                DefaultExt = ".pdf",
                Filter = "PDF files (.pdf)|*.pdf"
            };

            bool? result = dialog.ShowDialog();
            if (result != true) return;

            string path  = dialog.FileName;

            var window = (MainWindow)Window.GetWindow(this);
            var confirm = new ConfirmDialog($"Export your collection to:\n{path}?", window,"Export","Cancel");
            confirm.ShowDialog();
            if(!confirm.Confirmed) return;

            GeneratePdf(path);

        }








        //pdf generation
        private void GeneratePdf(string path)
        {
            using var db = new AppDbContext();
            var allGames = db.Games.Include(g => g.Achievements).ToList();
            var playedGames = allGames.Where(g => g.Status != GameStatus.Wishlisted).ToList();
            var gamesWithAchievements = allGames.Where(g => g.Achievements?.Count > 0).ToList();

            var document = new PdfDocument();
            document.Info.Title = "Dilettante Collection Export";

            // ── Fonts & Colours ──────────────────────────────────────────
            var titleFont = new XFont("Arial", 22, XFontStyleEx.Bold);
            var headingFont = new XFont("Arial", 13, XFontStyleEx.Bold);
            var labelFont = new XFont("Arial", 9, XFontStyleEx.Regular);
            var valueFont = new XFont("Arial", 11, XFontStyleEx.Bold);
            var smallFont = new XFont("Arial", 8, XFontStyleEx.Italic);
            var gameTitleFont = new XFont("Arial", 14, XFontStyleEx.Bold);
            var gameSubFont = new XFont("Arial", 9, XFontStyleEx.Regular);

            var bgBrush = new XSolidBrush(XColor.FromArgb(10, 22, 40));
            var cardBrush = new XSolidBrush(XColor.FromArgb(13, 31, 48));
            var accentBrush = new XSolidBrush(XColor.FromArgb(79, 195, 247));
            var accentPen = new XPen(XColor.FromArgb(79, 195, 247), 0.5);
            var whiteBrush = new XSolidBrush(XColors.White);
            var mutedBrush = new XSolidBrush(XColor.FromArgb(136, 153, 170));
            var greenBrush = new XSolidBrush(XColor.FromArgb(76, 175, 80));
            var redBrush = new XSolidBrush(XColor.FromArgb(239, 83, 80));

            double margin = 36;

            // ── Helper: draw background ──────────────────────────────────
            PdfPage NewPage()
            {
                var p = document.AddPage();
                p.Size = PdfSharp.PageSize.A4;
                var g = XGraphics.FromPdfPage(p);
                g.DrawRectangle(bgBrush, 0, 0, p.Width, p.Height);
                g.Dispose();
                return p;
            }

            // ── Page 1 — Profile Summary ─────────────────────────────────
            var summaryPage = NewPage();
            var gfx = XGraphics.FromPdfPage(summaryPage);
            double pw = summaryPage.Width;
            double y = margin;

            // Title block
            gfx.DrawString("Dilettante", titleFont, whiteBrush,
                new XRect(margin, y, pw - margin * 2, 36), XStringFormats.TopLeft);
            y += 28;
            gfx.DrawString($"Collection Export  —  {DateTime.Now:dd MMM yyyy}",
                smallFont, mutedBrush,
                new XRect(margin, y, pw - margin * 2, 16), XStringFormats.TopLeft);
            y += 24;
            gfx.DrawLine(accentPen, margin, y, pw - margin, y);
            y += 20;

            // ── Stat card helper ─────────────────────────────────────────
            void DrawCard(string label, string value, double x, double cy, double w, XSolidBrush valueBrush = null)
            {
                gfx.DrawRoundedRectangle(cardBrush, x, cy, w, 52, 8, 8);
                gfx.DrawString(label, labelFont, mutedBrush,
                    new XRect(x, cy + 6, w, 14), XStringFormats.Center);
                gfx.DrawString(value, valueFont, valueBrush ?? whiteBrush,
                    new XRect(x, cy + 22, w, 22), XStringFormats.Center);
            }

            // Library section
            gfx.DrawString("LIBRARY", headingFont, accentBrush,
                new XRect(margin, y, pw, 18), XStringFormats.TopLeft);
            y += 22;

            double cardW = (pw - margin * 2 - 10 * 5) / 6;
            double cx = margin;
            var libraryStats = new[]
            {
        ("Total",     allGames.Count.ToString(),                                          (XSolidBrush)null),
        ("Playing",   allGames.Count(g => g.Status == GameStatus.Playing).ToString(),     new XSolidBrush(XColor.FromArgb(42, 122, 191))),
        ("Completed", allGames.Count(g => g.Status == GameStatus.Completed).ToString(),   greenBrush),
        ("Wishlisted",allGames.Count(g => g.Status == GameStatus.Wishlisted).ToString(),  new XSolidBrush(XColor.FromArgb(171, 71, 188))),
        ("Paused",    allGames.Count(g => g.Status == GameStatus.Paused).ToString(),      new XSolidBrush(XColor.FromArgb(255, 152, 0))),
        ("Abandoned", allGames.Count(g => g.Status == GameStatus.Abandoned).ToString(),   redBrush),
    };
            foreach (var (lbl, val, brush) in libraryStats)
            {
                DrawCard(lbl, val, cx, y, cardW, brush);
                cx += cardW + 10;
            }
            y += 66;

            // Achievements section
            gfx.DrawLine(accentPen, margin, y, pw - margin, y);
            y += 16;
            gfx.DrawString("ACHIEVEMENTS", headingFont, accentBrush,
                new XRect(margin, y, pw, 18), XStringFormats.TopLeft);
            y += 22;

            int totalAch = gamesWithAchievements.Sum(g => g.Achievements!.Count);
            int unlockedAch = gamesWithAchievements.Sum(g => g.Achievements!.Count(a => a.IsUnlocked));
            double avgComp = gamesWithAchievements.Count > 0
                ? gamesWithAchievements.Average(g =>
                    (double)g.Achievements!.Count(a => a.IsUnlocked) / g.Achievements.Count * 100)
                : 0;

            double achCardW = (pw - margin * 2 - 20) / 3;
            DrawCard("Total Unlocked", $"{unlockedAch} / {totalAch}", margin, y, achCardW, accentBrush);
            DrawCard("Avg Completion", $"{avgComp:F1}%", margin + achCardW + 10, y, achCardW, accentBrush);
            DrawCard("Games Tracked", gamesWithAchievements.Count.ToString(), margin + achCardW * 2 + 20, y, achCardW);
            y += 66;

            // Integrity section
            gfx.DrawLine(accentPen, margin, y, pw - margin, y);
            y += 16;
            gfx.DrawString("GAMER INTEGRITY", headingFont, accentBrush,
                new XRect(margin, y, pw, 18), XStringFormats.TopLeft);
            y += 22;

            int legitimate = playedGames.Count(g =>
                g.Ownership == GameOwnership.OwnedCopy ||
                g.Ownership == GameOwnership.Subscription);
            int seafared = playedGames.Count(g => g.Ownership == GameOwnership.Seafared);
            double integrity = playedGames.Count > 0
                ? (double)legitimate / playedGames.Count * 10 : 0;

            double intCardW = (pw - margin * 2 - 20) / 3;
            DrawCard("Integrity Score", $"{integrity:F1} / 10", margin, y, intCardW, greenBrush);
            DrawCard("Legitimate", legitimate.ToString(), margin + intCardW + 10, y, intCardW, greenBrush);
            DrawCard("Seafared", seafared.ToString(), margin + intCardW * 2 + 20, y, intCardW, redBrush);
            y += 66;

            string flavour = integrity switch
            {
                >= 9 => "Absolutely squeaky clean.",
                >= 7 => "Mostly legitimate.",
                >= 5 => "A complicated relationship with ownership.",
                >= 3 => "The seas call to you.",
                _ => "A true pirate of the high seas."
            };
            gfx.DrawString(flavour, smallFont, mutedBrush,
                new XRect(margin, y, pw - margin * 2, 16), XStringFormats.TopLeft);

            gfx.Dispose();

            // ── Game pages — 2 per page ───────────────────────────────────
            var gameList = allGames.OrderBy(g => g.Name).ToList();
            double gameAreaHeight = 340;
            double gameSpacing = 16;
            double gameY = margin;
            PdfPage gamePage = null;
            XGraphics ggfx = null;
            double gw = 0;

            for (int i = 0; i < gameList.Count; i++)
            {
                // New page every 2 games
                if (i % 2 == 0)
                {
                    ggfx?.Dispose();
                    gamePage = NewPage();
                    ggfx = XGraphics.FromPdfPage(gamePage);
                    gw = gamePage.Width;
                    gameY = margin;
                }

                var game = gameList[i];

                // Card background
                ggfx.DrawRoundedRectangle(cardBrush,
                    margin, gameY, gw - margin * 2, gameAreaHeight, 10, 10);

                double cardLeft = margin + 16;
                double cardRight = gw - margin - 16;
                double gy = gameY + 16;

                // Header image — left side
                double imgW = 220;
                double imgH = 103; // Steam capsule aspect ratio
                bool drewImage = false;
                try
                {
                    if (!string.IsNullOrEmpty(game.HeaderImageUrl))
                    {
                        using var http = new HttpClient();
                        var imgBytes = http.GetByteArrayAsync(game.HeaderImageUrl).Result;
                        using var ms = new System.IO.MemoryStream(imgBytes);
                        var xImg = XImage.FromStream(ms);
                        ggfx.DrawImage(xImg, cardLeft, gy, imgW, imgH);
                        drewImage = true;
                    }
                }
                catch { }

                // Game info — right of image
                double infoX = drewImage ? cardLeft + imgW + 16 : cardLeft;
                double infoW = cardRight - infoX;

                ggfx.DrawString(game.Name, gameTitleFont, whiteBrush,
                    new XRect(infoX, gy, infoW, 22), XStringFormats.TopLeft);
                gy += 24;

                ggfx.DrawString(game.Developers ?? "", gameSubFont, mutedBrush,
                    new XRect(infoX, gy, infoW, 14), XStringFormats.TopLeft);
                gy += 18;

                // Stats inline
                var gameStats = new[]
                {
            ("Status",    game.Status.ToString()),
            ("Ownership", game.Ownership.ToString()),
            ("Score",     game.Userscore.HasValue ? $"{game.Userscore}/10" : "—"),
        };

                foreach (var (lbl, val) in gameStats)
                {
                    ggfx.DrawString(lbl, labelFont, mutedBrush,
                        new XRect(infoX, gy, 80, 14), XStringFormats.TopLeft);
                    ggfx.DrawString(val, gameSubFont, whiteBrush,
                        new XRect(infoX + 82, gy, infoW - 82, 14), XStringFormats.TopLeft);
                    gy += 17;
                }

                // Achievements
                if (game.Achievements?.Count > 0)
                {
                    int total = game.Achievements.Count;
                    int unlocked = game.Achievements.Count(a => a.IsUnlocked);
                    double pct = (double)unlocked / total * 100;

                    ggfx.DrawString("Achievements", labelFont, mutedBrush,
                        new XRect(infoX, gy, 80, 14), XStringFormats.TopLeft);
                    ggfx.DrawString($"{unlocked} / {total}  ({pct:F0}%)", gameSubFont, whiteBrush,
                        new XRect(infoX + 82, gy, infoW - 82, 14), XStringFormats.TopLeft);
                    gy += 17;

                    // Mini progress bar
                    double barW = infoW;
                    double barH = 6;
                    double barY = gy + 4;
                    double fillW = barW * pct / 100;
                    ggfx.DrawRoundedRectangle(new XSolidBrush(XColor.FromArgb(30, 50, 70)),
                        infoX, barY, barW, barH, 3, 3);
                    if (fillW > 0)
                        ggfx.DrawRoundedRectangle(greenBrush,
                            infoX, barY, fillW, barH, 3, 3);
                }
                else
                {
                    ggfx.DrawString("Achievements", labelFont, mutedBrush,
                        new XRect(infoX, gy, 80, 14), XStringFormats.TopLeft);
                    ggfx.DrawString("Not tracked", gameSubFont, mutedBrush,
                        new XRect(infoX + 82, gy, infoW - 82, 14), XStringFormats.TopLeft);
                }

                // Date added — bottom of card
                ggfx.DrawString($"Added {game.DateAdded:dd MMM yyyy}",
                    smallFont, mutedBrush,
                    new XRect(cardLeft, gameY + gameAreaHeight - 20, gw - margin * 2 - 32, 14),
                    XStringFormats.TopLeft);

                gameY += gameAreaHeight + gameSpacing;
            }

            ggfx?.Dispose();

            document.Save(path);

            var successDialog = new Views.ConfirmDialog(
                $"Exported successfully to:\n{path}",
                (MainWindow)Window.GetWindow(this),"Okay");
            successDialog.ShowDialog();
        }

    }
}