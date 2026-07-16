using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using CertPrep.Features.Dashboard;
using CertPrep.Features.ExamCatalog;
using CertPrep.Features.Options;
using CertPrep.Features.Practice;
using CertPrep.Features.Practice.Views;
using CertPrep.Features.Results;
using CertPrep.Shared;
using CertPrep.Shell;
using CertPrep.Tests.Infrastructure;
using SkiaSharp;
using Xunit;

namespace CertPrep.Tests.Views;

public sealed class ActualWorkflowSnapshotTests
{
    [AvaloniaFact]
    public async Task True_false_question_renders_radio_choices_and_immediate_feedback()
    {
        await using var database = await TestDatabase.CreateAsync();
        var services = database.CreateServices(randomSeed: 41);
        services.Appearance.SetTheme(AppTheme.Dark);
        var exam = (await services.Catalog.GetExamSummariesAsync())
            .Single(candidate => candidate.Code == "AZ-900");
        var run = await services.Practice.StartAsync(
            exam.Id,
            PracticeMode.Study,
            exam.QuestionCount);
        var question = run.Questions.First(candidate => candidate.Kind == QuestionKind.TrueFalse);
        var practice = new PracticeViewModel(
            run with { CurrentQuestionIndex = 0, Questions = [question] },
            services.Practice,
            _ => Task.CompletedTask,
            _ => Task.CompletedTask);
        var window = new Window
        {
            Content = new PracticeView { DataContext = practice },
            Width = 1000,
            Height = 650
        };

        window.Show();
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(2, window.GetVisualDescendants().OfType<ToggleButton>()
            .Count(choice => Equals(choice.Tag, "AnswerChoice")));
        Assert.All(practice.Choices, choice => Assert.True(choice.UsesRadioIndicator));
        CaptureFrame(window, "21-practice-true-false.png", expectBrandPixel: false);

        var correctIds = await database.GetCorrectChoiceIdsAsync(practice.Choices.Select(choice => choice.Id));
        practice.Choices.Single(choice => correctIds.Contains(choice.Id)).IsSelected = true;
        await practice.SubmitCommand.ExecuteAsync(null);
        Assert.True(practice.ShowFeedback);
        Assert.True(practice.FeedbackIsCorrect);
        CaptureFrame(window, "22-practice-true-false-feedback.png", expectBrandPixel: false);
        window.Close();
    }

    [AvaloniaFact]
    public async Task Saved_session_card_resumes_draft_selection_after_leaving_the_practice_view()
    {
        await using var database = await TestDatabase.CreateAsync();
        var services = database.CreateServices(randomSeed: 7);
        services.Appearance.SetTheme(AppTheme.Dark);
        var shell = new ShellViewModel(services.Catalog, services.Progress, services.Practice, services.Mastery, services.Rewards, services.Ranks, services.Appearance, services.Importer);
        await shell.InitializeAsync();
        var dashboard = Assert.IsType<DashboardViewModel>(shell.CurrentPage);
        dashboard.Exams[0].OpenCommand.Execute(null);
        var setup = Assert.IsType<PracticeSetupViewModel>(shell.CurrentPage);
        await setup.StartCommand.ExecuteAsync(null);
        var practice = Assert.IsType<PracticeViewModel>(shell.CurrentPage);
        practice.Choices[0].IsSelected = true;
        var selectedChoiceId = practice.Choices[0].Id;
        await practice.ExitCommand.ExecuteAsync(null);

        dashboard = Assert.IsType<DashboardViewModel>(shell.CurrentPage);
        Assert.True(dashboard.HasActiveSessions);
        var savedSession = Assert.Single(dashboard.ActiveSessions);
        MainWindow window = new()
        {
            DataContext = shell,
            Width = 1180,
            Height = 760
        };
        window.Show();
        Capture(window, "16-resumable-session.png");

        await savedSession.ResumeCommand.ExecuteAsync(null);
        var resumed = Assert.IsType<PracticeViewModel>(shell.CurrentPage);
        Assert.True(resumed.Choices.Single(choice => choice.Id == selectedChoiceId).IsSelected);

        var replacementChoice = resumed.Choices.First(choice => choice.Id != selectedChoiceId);
        resumed.Choices.Single(choice => choice.Id == selectedChoiceId).IsSelected = false;
        replacementChoice.IsSelected = true;
        var closed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        window.Closed += (_, _) => closed.TrySetResult();
        window.Close();
        await closed.Task.WaitAsync(TimeSpan.FromSeconds(5));

        var restartedServices = database.CreateServices(randomSeed: 8);
        var active = Assert.Single(await restartedServices.Practice.GetActiveSessionsAsync());
        var restoredAfterClose = await restartedServices.Practice.ResumeAsync(active.SessionId);
        Assert.NotNull(restoredAfterClose.Run);
        var restoredQuestion = restoredAfterClose.Run.Questions[restoredAfterClose.Run.CurrentQuestionIndex];
        Assert.True(restoredQuestion.Choices.Single(choice => choice.SessionChoiceId == replacementChoice.Id).IsSelected);
        Assert.False(restoredQuestion.Choices.Single(choice => choice.SessionChoiceId == selectedChoiceId).IsSelected);
    }

    [AvaloniaFact]
    public async Task System_theme_is_default_and_minimum_size_renders_in_both_palettes()
    {
        await using var database = await TestDatabase.CreateAsync();
        var services = database.CreateServices();
        Assert.Equal(AppTheme.System, services.Appearance.CurrentTheme);
        Assert.Equal(ThemeVariant.Default, Application.Current!.RequestedThemeVariant);

        services.Appearance.SetTheme(AppTheme.Dark);
        var shell = new ShellViewModel(services.Catalog, services.Progress, services.Practice, services.Mastery, services.Rewards, services.Ranks, services.Appearance, services.Importer);
        await shell.InitializeAsync();
        MainWindow window = new()
        {
            DataContext = shell,
            Width = 920,
            Height = 640
        };

        window.Show();
        Capture(window, "14-dashboard-minimum-dark.png");
        shell.ShowOptionsCommand.Execute(null);
        var options = Assert.IsType<OptionsViewModel>(shell.CurrentPage);
        options.SelectedTheme = AppTheme.Light;
        Assert.Equal(ThemeVariant.Light, Application.Current.RequestedThemeVariant);
        Dispatcher.UIThread.RunJobs();
        var optionText = window.GetVisualDescendants()
            .OfType<TextBlock>()
            .Select(textBlock => textBlock.Text)
            .OfType<string>()
            .ToList();
        Assert.Contains(optionText, text => text.Contains("formal JSON Schema", StringComparison.Ordinal));
        Assert.Contains(optionText, text => text.Contains("Missing entries are not deleted", StringComparison.Ordinal));
        Capture(window, "20-options-minimum-light.png");
        await shell.ShowDashboardCommand.ExecuteAsync(null);
        Capture(window, "15-dashboard-minimum-light.png");
        window.Close();
    }

    [AvaloniaFact]
    public async Task Incorrect_study_feedback_renders_the_wrong_and_correct_answers_immediately()
    {
        await using var database = await TestDatabase.CreateAsync();
        var services = database.CreateServices(randomSeed: 31);
        services.Appearance.SetTheme(AppTheme.Dark);
        var shell = new ShellViewModel(services.Catalog, services.Progress, services.Practice, services.Mastery, services.Rewards, services.Ranks, services.Appearance, services.Importer);
        await shell.InitializeAsync();
        var dashboard = Assert.IsType<DashboardViewModel>(shell.CurrentPage);
        dashboard.Exams[0].OpenCommand.Execute(null);
        var setup = Assert.IsType<PracticeSetupViewModel>(shell.CurrentPage);
        await setup.StartCommand.ExecuteAsync(null);
        var practice = Assert.IsType<PracticeViewModel>(shell.CurrentPage);
        var correctIds = await database.GetCorrectChoiceIdsAsync(practice.Choices.Select(choice => choice.Id));
        SelectIncorrectAnswers(practice, correctIds);
        await practice.SubmitCommand.ExecuteAsync(null);
        Assert.True(practice.ShowFeedback);
        Assert.False(practice.FeedbackIsCorrect);
        Assert.Contains(practice.Choices, choice => choice.ShowIncorrectIndicator);
        Assert.Contains(practice.Choices, choice => choice.ShowCorrectIndicator);

        MainWindow window = new()
        {
            DataContext = shell,
            Width = 1280,
            Height = 800
        };
        window.Show();
        Capture(window, "19-practice-incorrect.png");
        window.Close();
    }

    [AvaloniaFact]
    public async Task Real_views_render_and_a_complete_study_run_reaches_progress()
    {
        await using var database = await TestDatabase.CreateAsync();
        var services = database.CreateServices(randomSeed: 19);
        services.Appearance.SetTheme(AppTheme.Dark);
        var shell = new ShellViewModel(services.Catalog, services.Progress, services.Practice, services.Mastery, services.Rewards, services.Ranks, services.Appearance, services.Importer);
        await shell.InitializeAsync();

        MainWindow window = new()
        {
            DataContext = shell,
            Width = 1280,
            Height = 800
        };

        window.Show();
        window = Capture(window, "01-dashboard.png");

        var optionsButton = window.GetVisualDescendants()
            .OfType<Button>()
            .Single(button => Equals(button.Tag, "OptionsPage"));
        optionsButton.Command!.Execute(null);
        Assert.IsType<OptionsViewModel>(shell.CurrentPage);
        window = Capture(window, "17-options-page.png");
        await shell.ShowDashboardCommand.ExecuteAsync(null);
        Dispatcher.UIThread.RunJobs();

        var startButton = window.GetVisualDescendants()
            .OfType<Button>()
            .First(button => Equals(button.Tag, "StartPractice"));
        startButton.Focus();
        window.KeyPressQwerty(PhysicalKey.Space, RawInputModifiers.None);
        window.KeyReleaseQwerty(PhysicalKey.Space, RawInputModifiers.None);
        Assert.IsType<PracticeSetupViewModel>(shell.CurrentPage);
        window = Capture(window, "02-practice-setup.png");

        var setup = (PracticeSetupViewModel)shell.CurrentPage!;
        await setup.StartCommand.ExecuteAsync(null);
        var practice = Assert.IsType<PracticeViewModel>(shell.CurrentPage);
        Assert.All(practice.Choices, choice => Assert.True(choice.UsesRadioIndicator));
        Dispatcher.UIThread.RunJobs();
        var firstChoice = window.GetVisualDescendants()
            .OfType<ToggleButton>()
            .First(choice => Equals(choice.Tag, "AnswerChoice"));
        var hoverPoint = firstChoice.TranslatePoint(
            new Point(firstChoice.Bounds.Width / 2, firstChoice.Bounds.Height / 2),
            window);
        Assert.NotNull(hoverPoint);
        window.MouseMove(hoverPoint.Value, RawInputModifiers.None);
        window = Capture(window, "03-practice-question.png");
        window.MouseMove(new Point(0, 0), RawInputModifiers.None);

        var capturedSelection = false;
        var capturedFeedback = false;
        while (shell.CurrentPage is PracticeViewModel currentPractice)
        {
            var correctIds = await database.GetCorrectChoiceIdsAsync(currentPractice.Choices.Select(choice => choice.Id));
            foreach (var choice in currentPractice.Choices.Where(choice => correctIds.Contains(choice.Id)))
            {
                choice.IsSelected = true;
            }

            if (!capturedSelection)
            {
                window = Capture(window, "18-practice-selected.png");
                capturedSelection = true;
            }

            await currentPractice.SubmitCommand.ExecuteAsync(null);
            if (shell.CurrentPage is not PracticeViewModel)
            {
                break;
            }

            if (!capturedFeedback)
            {
                window = Capture(window, "04-practice-feedback.png");
                capturedFeedback = true;
            }

            await currentPractice.NextCommand.ExecuteAsync(null);
        }

        var results = Assert.IsType<ResultsViewModel>(shell.CurrentPage);
        Assert.Equal("100%", results.ScoreText);
        window = Capture(window, "05-results.png");

        await results.ProgressCommand.ExecuteAsync(null);
        window = Capture(window, "06-progress.png");

        window.Close();
    }

    [AvaloniaFact]
    public async Task Mixed_exam_workflow_renders_review_retry_and_momentum_views()
    {
        await using var database = await TestDatabase.CreateAsync();
        var services = database.CreateServices(randomSeed: 29);
        services.Appearance.SetTheme(AppTheme.Dark);
        var shell = new ShellViewModel(services.Catalog, services.Progress, services.Practice, services.Mastery, services.Rewards, services.Ranks, services.Appearance, services.Importer);
        await shell.InitializeAsync();

        MainWindow window = new()
        {
            DataContext = shell,
            Width = 1280,
            Height = 800
        };

        window.Show();
        var mixedButton = window.GetVisualDescendants()
            .OfType<Button>()
            .Single(button => Equals(button.Tag, "BuildMixedSession"));
        mixedButton.Focus();
        window.KeyPressQwerty(PhysicalKey.Space, RawInputModifiers.None);
        window.KeyReleaseQwerty(PhysicalKey.Space, RawInputModifiers.None);

        var setup = Assert.IsType<MixedPracticeSetupViewModel>(shell.CurrentPage);
        Assert.Equal(2, setup.SelectedExamCount);
        window = Capture(window, "07-mixed-setup.png");

        setup.SelectedMode = setup.ModeOptions.Single(option => option.Mode == PracticeMode.ExamSimulation);
        setup.SelectedQuestionCount = setup.QuestionCountOptions.Single(option => option.Count == 5);
        await setup.StartCommand.ExecuteAsync(null);

        var practice = Assert.IsType<PracticeViewModel>(shell.CurrentPage);
        Assert.Contains("of 5", practice.QuestionPosition);
        Assert.Contains("•", practice.ObjectiveName);
        window = Capture(window, "08-mixed-question.png");

        var firstCorrectIds = await database.GetCorrectChoiceIdsAsync(practice.Choices.Select(choice => choice.Id));
        SelectIncorrectAnswers(practice, firstCorrectIds);
        await practice.SubmitCommand.ExecuteAsync(null);
        Assert.False(practice.ShowFeedback);

        while (shell.CurrentPage is PracticeViewModel currentPractice)
        {
            var correctIds = await database.GetCorrectChoiceIdsAsync(currentPractice.Choices.Select(choice => choice.Id));
            foreach (var choice in currentPractice.Choices.Where(choice => correctIds.Contains(choice.Id)))
            {
                choice.IsSelected = true;
            }

            await currentPractice.SubmitCommand.ExecuteAsync(null);
        }

        var results = Assert.IsType<ResultsViewModel>(shell.CurrentPage);
        Assert.Equal("80%", results.ScoreText);
        Assert.True(results.HasMissedQuestions);
        var missed = Assert.Single(results.MissedQuestions);
        Assert.False(string.IsNullOrWhiteSpace(missed.Explanation));
        Assert.False(string.IsNullOrWhiteSpace(missed.CorrectAnswersText));
        window = Capture(window, "09-mixed-results.png");
        ScrollPageToBottom(window);
        window = Capture(window, "10-missed-review.png");

        await results.RetryMissedCommand.ExecuteAsync(null);
        var retry = Assert.IsType<PracticeViewModel>(shell.CurrentPage);
        Assert.Equal("Question 1 of 1", retry.QuestionPosition);
        window = Capture(window, "11-retry-missed.png");

        var retryCorrectIds = await database.GetCorrectChoiceIdsAsync(retry.Choices.Select(choice => choice.Id));
        foreach (var choice in retry.Choices.Where(choice => retryCorrectIds.Contains(choice.Id)))
        {
            choice.IsSelected = true;
        }

        await retry.SubmitCommand.ExecuteAsync(null);
        Assert.True(retry.ShowFeedback);
        Assert.False(string.IsNullOrWhiteSpace(retry.Explanation));
        ScrollPageToBottom(window);
        window = Capture(window, "12-retry-feedback.png");
        await retry.NextCommand.ExecuteAsync(null);

        var retryResults = Assert.IsType<ResultsViewModel>(shell.CurrentPage);
        Assert.Equal("100%", retryResults.ScoreText);
        await retryResults.DashboardCommand.ExecuteAsync(null);
        var dashboard = Assert.IsType<DashboardViewModel>(shell.CurrentPage);
        Assert.Contains("Cross-exam", dashboard.UnlockedBadgesText);
        Assert.Contains("Clean sweep", dashboard.UnlockedBadgesText);
        window = Capture(window, "13-dashboard-momentum.png");

        window.Close();
    }

    private static MainWindow Capture(MainWindow window, string fileName)
    {
        CaptureFrame(window, fileName, expectBrandPixel: true);
        return window;
    }

    private static void CaptureFrame(Window window, string fileName, bool expectBrandPixel)
    {
        var outputDirectory = Path.Combine(FindRepositoryRoot(), "artifacts", "ui-snapshots");
        Directory.CreateDirectory(outputDirectory);
        var path = Path.Combine(outputDirectory, fileName);

        var snapshotWindow = window;
        byte[]? bestFrame = null;
        for (var attempt = 0; attempt < 3; attempt++)
        {
            InvalidateVisualTree(snapshotWindow);
            Dispatcher.UIThread.RunJobs();

            using var frame = snapshotWindow.CaptureRenderedFrame();
            Assert.NotNull(frame);
            using var encodedFrame = new MemoryStream();
            frame.Save(encodedFrame, PngBitmapEncoderOptions.Default);
            if (bestFrame is null || encodedFrame.Length > bestFrame.Length)
            {
                bestFrame = encodedFrame.ToArray();
            }
        }

        Assert.NotNull(bestFrame);
        File.WriteAllBytes(path, bestFrame);

        var file = new FileInfo(path);
        Assert.True(file.Exists);
        Assert.True(file.Length > 10_000, $"Rendered frame was unexpectedly small: {file.Length} bytes.");
        using var renderedImage = SKBitmap.Decode(path);
        Assert.NotNull(renderedImage);
        if (expectBrandPixel)
        {
            var brandPixel = renderedImage.GetPixel(40, 40);
            Assert.True(
                brandPixel.Blue >= 0xD0 && brandPixel.Green >= 0x60 && brandPixel.Red <= 0x60,
                $"Expected the CertPrep icon's blue mark at (40, 40), but found {brandPixel}.");
        }
    }

    private static void InvalidateVisualTree(Control root)
    {
        root.InvalidateMeasure();
        root.InvalidateArrange();
        root.InvalidateVisual();
        foreach (var control in root.GetVisualDescendants().OfType<Control>())
        {
            control.InvalidateMeasure();
            control.InvalidateArrange();
            control.InvalidateVisual();
        }
    }

    private static void ScrollPageToBottom(MainWindow window)
    {
        Dispatcher.UIThread.RunJobs();
        var scrollViewer = window.GetVisualDescendants()
            .OfType<ScrollViewer>()
            .OrderByDescending(viewer => viewer.Extent.Height - viewer.Viewport.Height)
            .FirstOrDefault(viewer => viewer.Extent.Height > viewer.Viewport.Height);
        if (scrollViewer is null)
        {
            return;
        }

        scrollViewer.Offset = new Vector(0, scrollViewer.Extent.Height - scrollViewer.Viewport.Height);
        Dispatcher.UIThread.RunJobs();
    }

    private static void SelectIncorrectAnswers(
        PracticeViewModel practice,
        IReadOnlySet<int> correctIds)
    {
        var selectedIds = correctIds.ToHashSet();
        selectedIds.Remove(selectedIds.First());
        selectedIds.Add(practice.Choices.First(choice => !correctIds.Contains(choice.Id)).Id);
        foreach (var choice in practice.Choices.Where(choice => selectedIds.Contains(choice.Id)))
        {
            choice.IsSelected = true;
        }
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (Directory.Exists(Path.Combine(directory.FullName, ".git")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate the repository root.");
    }
}
