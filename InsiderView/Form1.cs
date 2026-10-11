using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Drawing2D;

namespace InsiderView
{
    public partial class Form1 : Form
    {
        private const string AppName = "Insider View";

        // One file found by the search, its row in the left list, and what was read inside it.
        private sealed class Project
        {
            public Project(string filePath, long size, DateTime modified)
            {
                FilePath = filePath;
                Size = size;
                Modified = modified;
            }

            public string FilePath { get; set; }   // changes when the file is moved
            public long Size { get; }
            public DateTime Modified { get; }
            public ListViewItem? Row { get; set; }

            /// <summary>Null until the background reading has reached this file.</summary>
            public InsideResult? Inside { get; set; }
        }

        /// <summary>A file the search found, with what Windows already knows about it.</summary>
        internal sealed record FoundFile(string Path, long Size, DateTime Modified);

        private enum Order { AsFound, Name, Newest, Oldest, Largest, Smallest }

        // Pictures for the left list: the Windows icon of each file type, and the tick that
        // marks a project with a search match. Picture stores, not controls.
        private readonly ImageList _fileIcons = new() { ColorDepth = ColorDepth.Depth32Bit };
        private readonly ImageList _ticks = new() { ColorDepth = ColorDepth.Depth32Bit };

        private readonly List<Project> _projects = new();   // everything the last search found, in the order found
        private CancellationTokenSource? _run;               // the search in progress, if any
        private Project? _shown;                             // the project whose contents are on the right
        private string _extension = string.Empty;            // what the last search looked for
        private bool _reading;                               // the background reading is still going
        private int _read;                                   // how many files it has read so far
        private bool _hitsOnly;                              // "Show hits" is on
        private Order _order = Order.AsFound;
        private bool _refreshingList;                        // the left list is being rebuilt
        private bool _statusQueued;

        public Form1()
        {
            InitializeComponent();
            Text = AppName;
            PrepareLists();

            BtnSelectFolder.Click += (_, _) => SelectFolder();
            btnStartIndex.Click += async (_, _) => await StartIndexAsync();
            tbxSearchText.TextChanged += (_, _) => ApplySearch();
            lvProjecsFound.SelectedIndexChanged += (_, _) => ProjectSelectionChanged();

            // Filtering.
            button1.Click += (_, _) => ShowHitsOnly(true);          // "Show hits"
            button2.Click += (_, _) => ShowHitsOnly(false);         // "Show all"

            // Sorting.
            button3.Click += (_, _) => SortBy(Order.Name);          // "A - Z"
            button4.Click += (_, _) => SortBy(Order.Newest);        // "Newest first"
            button5.Click += (_, _) => SortBy(Order.Oldest);        // "Oldest First"
            button6.Click += (_, _) => SortBy(Order.Largest);       // "Largest First"
            button7.Click += (_, _) => SortBy(Order.Smallest);      // "Smallest First"

            // What to do with the selected projects.
            btnOpenLocation.Click += (_, _) => OpenInExplorer();                     // "Open In Explorer"
            btnCopySlected.Click += (_, _) => CopyLocation();                        // "Copy location"
            button8.Click += async (_, _) => await CopyOrMoveAsync(move: false);     // "Copy to..."
            button9.Click += async (_, _) => await CopyOrMoveAsync(move: true);      // "Move to..."
            btnRunWithDefaultApp.Click += (_, _) => RunWithDefaultApp();             // "Run with default"

            btnClose.Click += (_, _) => Close();
            FormClosing += (_, _) => _run?.Cancel();
        }

        // ---- the two lists -------------------------------------------------------------------

        // A list is only set up here while the designer has left it without columns. Give a
        // list its columns in the designer and it is used exactly as designed.
        private void PrepareLists()
        {
            Size iconSize = SystemInformation.SmallIconSize;
            _fileIcons.ImageSize = iconSize;
            _ticks.ImageSize = iconSize;
            _ticks.Images.Add(TickPicture(iconSize));

            if (lvProjecsFound.Columns.Count == 0)
            {
                const int nameWidth = 180, sizeWidth = 70, modifiedWidth = 115;
                int locationWidth = lvProjecsFound.ClientSize.Width - nameWidth - sizeWidth - modifiedWidth
                    - SystemInformation.VerticalScrollBarWidth - 4;

                lvProjecsFound.View = View.Details;
                lvProjecsFound.FullRowSelect = true;
                lvProjecsFound.HideSelection = false;
                lvProjecsFound.Columns.Add("Project", nameWidth);
                lvProjecsFound.Columns.Add("Location", Math.Max(100, locationWidth));
                lvProjecsFound.Columns.Add("Size", sizeWidth, HorizontalAlignment.Right);
                lvProjecsFound.Columns.Add("Modified", modifiedWidth);
            }

            lvProjecsFound.ShowItemToolTips = true;
            lvProjecsFound.SmallImageList = _fileIcons;
            lvProjecsFound.StateImageList = _ticks;

            if (lvInside.Columns.Count == 0)
            {
                lvInside.View = View.Details;
                lvInside.FullRowSelect = true;
                lvInside.HideSelection = false;
                lvInside.Columns.Add("Inside",
                    Math.Max(120, lvInside.ClientSize.Width - SystemInformation.VerticalScrollBarWidth - 4));
            }
        }

        private static Bitmap TickPicture(Size size)
        {
            var picture = new Bitmap(size.Width, size.Height);
            using Graphics g = Graphics.FromImage(picture);
            g.SmoothingMode = SmoothingMode.AntiAlias;

            float w = size.Width, h = size.Height;
            using var pen = new Pen(Color.FromArgb(0, 140, 60), Math.Max(2f, w / 7f))
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round,
                LineJoin = LineJoin.Round
            };
            g.DrawLines(pen, new[]
            {
                new PointF(w * 0.18f, h * 0.55f),
                new PointF(w * 0.42f, h * 0.78f),
                new PointF(w * 0.84f, h * 0.24f)
            });

            return picture;
        }

        // ---- choosing the folder and starting ------------------------------------------------

        private void SelectFolder()
        {
            using var picker = new FolderBrowserDialog
            {
                Description = "Folder to search",
                UseDescriptionForTitle = true
            };

            string current = tbxTargetLocation.Text.Trim();
            if (Directory.Exists(current)) picker.SelectedPath = current;

            if (picker.ShowDialog(this) == DialogResult.OK)
                tbxTargetLocation.Text = picker.SelectedPath;
        }

        // "ai", ".ai" and "*.ai" all mean ".ai". Anything that is not a plain extension gives "".
        internal static string CleanExtension(string typed)
        {
            string extension = typed.Trim().TrimStart('*');
            if (extension.Length == 0) return string.Empty;
            if (!extension.StartsWith('.')) extension = "." + extension;
            if (extension.Length < 2) return string.Empty;
            if (extension.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 || extension.Contains(' ')) return string.Empty;
            if (extension.IndexOf('.', 1) >= 0) return string.Empty;
            return extension;
        }

        private async Task StartIndexAsync()
        {
            string folder = tbxTargetLocation.Text.Trim().Trim('"');
            string extension = CleanExtension(tbxExtension.Text);

            if (folder.Length == 0 || !Directory.Exists(folder))
            {
                Tell("Choose the folder to search first.");
                return;
            }

            if (extension.Length == 0)
            {
                Tell("Type the kind of file to look for, for example .ai");
                return;
            }

            // A hand-typed folder can have forward slashes or ".." in it. Windows' own icon and
            // Explorer calls want the plain full path.
            folder = Path.GetFullPath(folder);

            // Starting again abandons a search that is still running.
            _run?.Cancel();
            var run = new CancellationTokenSource();
            _run = run;

            _projects.Clear();
            lvProjecsFound.Items.Clear();
            lvInside.Items.Clear();
            _fileIcons.Images.Clear();
            _shown = null;
            _extension = extension;
            _read = 0;
            _reading = false;
            ShowStatus($"looking for {extension} files...");

            try
            {
                // 1. Find the files. Only the running count comes back while it works.
                var counted = new Progress<int>(count =>
                {
                    if (ReferenceEquals(_run, run)) ShowStatus($"looking for {extension} files... {count:N0} found");
                });
                bool subfolders = chkBxRecurseiveSearch.Checked;
                List<FoundFile> found = await Task.Run(() => FindFiles(folder, extension, subfolders, counted, run.Token), run.Token);
                if (!ReferenceEquals(_run, run)) return;

                // 2. List them.
                AddProjects(found);
                if (_projects.Count == 0)
                {
                    ShowStatus(subfolders
                        ? $"no {extension} files found in that folder or below it"
                        : $"no {extension} files found in that folder (Recursive search is off)");
                    return;
                }

                // 3. Read what is inside each one, in the background.
                _reading = true;
                UpdateStatus();
                List<Project> toRead = _projects.ToList();
                await RunOnItsOwnThread(() => ReadAll(toRead, run));
                if (!ReferenceEquals(_run, run)) return;

                _reading = false;
                UpdateStatus();
            }
            catch (OperationCanceledException)
            {
                // A newer search, or the window closing, took over.
            }
            catch (Exception ex)
            {
                if (ReferenceEquals(_run, run))
                {
                    _reading = false;
                    ShowStatus("the search stopped");
                    Tell("The search stopped: " + ex.Message);
                }
            }
        }

        // Every file with the extension in the folder - and, with "Recursive search" ticked, in
        // everything below it. Folders that Windows will not let us read are skipped without
        // stopping the search.
        internal static List<FoundFile> FindFiles(string folder, string extension, bool subfolders, IProgress<int>? counted, CancellationToken stop)
        {
            var options = new EnumerationOptions
            {
                RecurseSubdirectories = subfolders,
                IgnoreInaccessible = true,
                AttributesToSkip = FileAttributes.System,
                MaxRecursionDepth = 64
            };

            var found = new List<FoundFile>();
            foreach (FileInfo file in new DirectoryInfo(folder).EnumerateFiles("*" + extension, options))
            {
                stop.ThrowIfCancellationRequested();
                if (!file.Extension.Equals(extension, StringComparison.OrdinalIgnoreCase)) continue;

                found.Add(new FoundFile(file.FullName, file.Length, file.LastWriteTime));
                if (found.Count % 250 == 0) counted?.Report(found.Count);
            }

            return found;
        }

        // Makes a project and its list row for every file found, then shows them.
        private void AddProjects(List<FoundFile> found)
        {
            foreach (FoundFile file in found)
            {
                var project = new Project(file.Path, file.Size, file.Modified);
                var row = new ListViewItem(Path.GetFileName(file.Path)) { Tag = project, ToolTipText = file.Path };
                row.SubItems.Add(Path.GetDirectoryName(file.Path) ?? string.Empty);
                row.SubItems.Add(SizeText(file.Size));
                row.SubItems.Add(file.Modified.ToString("dd MMM yyyy HH:mm"));

                // A type with one icon for every file gets it now. A type whose files each carry
                // their own icon (.exe and the like) gets it as the file is read.
                string iconKey = ShellIcons.KeyFor(file.Path);
                if (iconKey != file.Path)
                {
                    AddIcon(iconKey, () => ShellIcons.Small(file.Path));
                    row.ImageKey = iconKey;
                }

                project.Row = row;
                _projects.Add(project);
            }

            RefreshProjectList();
        }

        private void AddIcon(string key, Func<Bitmap?> fetch)
        {
            if (_fileIcons.Images.ContainsKey(key)) return;

            using Bitmap? icon = fetch();
            if (icon != null) _fileIcons.Images.Add(key, icon);
        }

        internal static string SizeText(long bytes)
        {
            if (bytes < 1024) return bytes + " B";
            if (bytes < 1024 * 1024) return (bytes / 1024.0).ToString("0") + " KB";
            if (bytes < 1024L * 1024 * 1024) return (bytes / (1024.0 * 1024)).ToString("0.0") + " MB";
            return (bytes / (1024.0 * 1024 * 1024)).ToString("0.00") + " GB";
        }

        // ---- reading the files in the background ---------------------------------------------

        // Windows wants this kind of thread for fetching a file's own icon.
        private static Task RunOnItsOwnThread(Action work)
        {
            var finished = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var thread = new Thread(() =>
            {
                try
                {
                    work();
                    finished.SetResult();
                }
                catch (OperationCanceledException)
                {
                    finished.SetCanceled();
                }
                catch (Exception ex)
                {
                    finished.SetException(ex);
                }
            })
            {
                IsBackground = true,
                Name = "Insider View reading"
            };
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            return finished.Task;
        }

        // Runs on the background thread. Results go back to the window a batch at a time, so a
        // folder with thousands of files cannot swamp it with one message per file.
        private void ReadAll(List<Project> projects, CancellationTokenSource run)
        {
            var ready = new List<(Project Project, Bitmap? OwnIcon)>();
            var sinceLastBatch = Stopwatch.StartNew();

            foreach (Project project in projects)
            {
                run.Token.ThrowIfCancellationRequested();

                string path = project.FilePath;
                project.Inside = InsideReaders.Read(path);
                Bitmap? ownIcon = ShellIcons.KeyFor(path) == path ? ShellIcons.Small(path) : null;
                ready.Add((project, ownIcon));

                if (ready.Count >= 64 || sinceLastBatch.ElapsedMilliseconds >= 120)
                {
                    SendBatch(ready, run);
                    ready = new List<(Project, Bitmap?)>();
                    sinceLastBatch.Restart();
                }
            }

            SendBatch(ready, run);
        }

        private void SendBatch(List<(Project Project, Bitmap? OwnIcon)> batch, CancellationTokenSource run)
        {
            if (batch.Count == 0) return;

            try
            {
                BeginInvoke(() => ShowRead(batch, run));
            }
            catch (InvalidOperationException)
            {
                // The window has closed.
            }
        }

        // Back on the window's thread: these files have now been read.
        private void ShowRead(List<(Project Project, Bitmap? OwnIcon)> batch, CancellationTokenSource run)
        {
            // A batch from a search that has since been replaced, or one arriving as the window
            // closes, is thrown away.
            if (!ReferenceEquals(_run, run) || IsDisposed)
            {
                foreach ((_, Bitmap? unused) in batch) unused?.Dispose();
                return;
            }

            string find = tbxSearchText.Text.Trim();
            bool newHit = false;

            lvProjecsFound.BeginUpdate();
            foreach ((Project project, Bitmap? ownIcon) in batch)
            {
                using (ownIcon)
                {
                    if (project.Row == null) continue;

                    if (ownIcon != null)
                    {
                        _fileIcons.Images.Add(project.FilePath, ownIcon);
                        project.Row.ImageKey = project.FilePath;
                    }

                    bool hit = Matches(project, find);
                    project.Row.StateImageIndex = hit ? 0 : -1;
                    newHit |= hit;
                    if (ReferenceEquals(project, _shown)) ShowInside(project);
                }
            }
            lvProjecsFound.EndUpdate();

            _read += batch.Count;

            // With "Show hits" on, a file that has just turned out to be a hit has to appear.
            if (_hitsOnly && newHit) RefreshProjectList();
            UpdateStatus();
        }

        // ---- searching, filtering, sorting ---------------------------------------------------

        private static bool Matches(Project project, string find)
            => find.Length > 0
               && project.Inside != null
               && project.Inside.Names.Any(name => name.Contains(find, StringComparison.OrdinalIgnoreCase));

        // A tick beside every project that has a matching name inside it. No search text, no ticks.
        private void ApplySearch()
        {
            string find = tbxSearchText.Text.Trim();

            lvProjecsFound.BeginUpdate();
            foreach (Project project in _projects)
            {
                if (project.Row == null) continue;

                int tick = Matches(project, find) ? 0 : -1;
                if (project.Row.StateImageIndex != tick) project.Row.StateImageIndex = tick;
            }
            lvProjecsFound.EndUpdate();

            if (_hitsOnly) RefreshProjectList();
            UpdateStatus();
        }

        // "Show hits" leaves only the ticked projects in the list; "Show all" brings the rest back.
        private void ShowHitsOnly(bool hitsOnly)
        {
            _hitsOnly = hitsOnly;
            RefreshProjectList();
            UpdateStatus();
        }

        private void SortBy(Order order)
        {
            _order = order;
            RefreshProjectList();
        }

        // Puts the projects in the left list: all of them or only the hits, in the chosen order.
        // The rows themselves are kept, so ticks, icons and the selection carry over.
        private void RefreshProjectList()
        {
            string find = tbxSearchText.Text.Trim();
            IEnumerable<Project> wanted = _hitsOnly ? _projects.Where(p => Matches(p, find)) : _projects;

            wanted = _order switch
            {
                Order.Name => wanted.OrderBy(p => Path.GetFileName(p.FilePath), StringComparer.OrdinalIgnoreCase)
                                    .ThenBy(p => p.FilePath, StringComparer.OrdinalIgnoreCase),
                Order.Newest => wanted.OrderByDescending(p => p.Modified),
                Order.Oldest => wanted.OrderBy(p => p.Modified),
                Order.Largest => wanted.OrderByDescending(p => p.Size),
                Order.Smallest => wanted.OrderBy(p => p.Size),
                _ => wanted
            };

            ListViewItem[] rows = wanted.Select(p => p.Row).OfType<ListViewItem>().ToArray();
            var selected = new HashSet<Project>(SelectedProjects());
            Project? shown = _shown;

            _refreshingList = true;
            lvProjecsFound.BeginUpdate();
            try
            {
                lvProjecsFound.Items.Clear();
                lvProjecsFound.Items.AddRange(rows);
                foreach (ListViewItem row in rows)
                {
                    if (row.Tag is Project project && selected.Contains(project)) row.Selected = true;
                }

                // Keep the project on the right as the focused one if it is still listed.
                if (shown?.Row is { ListView: not null } shownRow && shownRow.Selected) shownRow.Focused = true;
            }
            finally
            {
                lvProjecsFound.EndUpdate();
                _refreshingList = false;
            }

            ShowSelectedProject();
        }

        // ---- the status line -----------------------------------------------------------------

        private void ShowStatus(string text) => lblStatus.Text = "Status: " + text;

        // How many files, their size, how the reading is going, hits for the search, and what is
        // selected.
        private void UpdateStatus()
        {
            _statusQueued = false;
            if (_projects.Count == 0) return;

            var parts = new List<string>
            {
                _projects.Count == 1 ? $"1 {_extension} file" : $"{_projects.Count:N0} {_extension} files",
                SizeText(_projects.Sum(p => p.Size))
            };

            if (_reading)
                parts.Add($"reading {_read:N0} of {_projects.Count:N0}");
            else if (InsideReaders.CanRead(_extension))
            {
                int names = _projects.Sum(p => p.Inside?.Names.Count ?? 0);
                parts.Add(names == 1 ? "1 name inside" : $"{names:N0} names inside");
            }
            else
                parts.Add($"cannot look inside {_extension} files yet");

            string find = tbxSearchText.Text.Trim();
            if (find.Length > 0)
                parts.Add($"{_projects.Count(p => Matches(p, find)):N0} with hits");

            List<Project> selected = SelectedProjects();
            if (selected.Count > 0)
                parts.Add($"{selected.Count:N0} selected ({SizeText(selected.Sum(p => p.Size))})");

            ShowStatus(string.Join(", ", parts));
        }

        // Selecting a few thousand rows at once raises this once per row; the status line is
        // worked out once, after they have all arrived.
        private void QueueStatus()
        {
            if (_statusQueued || !IsHandleCreated) return;

            _statusQueued = true;
            BeginInvoke(UpdateStatus);
        }

        // ---- the right-hand list -------------------------------------------------------------

        private void ProjectSelectionChanged()
        {
            if (_refreshingList) return;

            ShowSelectedProject();
            QueueStatus();
        }

        private void ShowSelectedProject()
        {
            ListViewItem? row = lvProjecsFound.FocusedItem is { Selected: true } focused
                ? focused
                : lvProjecsFound.SelectedItems.Count > 0 ? lvProjecsFound.SelectedItems[0] : null;

            Project? project = row?.Tag as Project;
            if (!ReferenceEquals(project, _shown) || project == null) ShowInside(project);
        }

        private void ShowInside(Project? project)
        {
            _shown = project;

            lvInside.BeginUpdate();
            lvInside.Items.Clear();
            if (project != null)
            {
                if (project.Inside == null) AddNote("Still being read...");
                else if (project.Inside.Problem != null) AddNote(project.Inside.Problem);
                else if (project.Inside.Names.Count == 0) AddNote("Nothing was found inside this file.");
                else lvInside.Items.AddRange(project.Inside.Names.Select(name => new ListViewItem(name)).ToArray());
            }
            lvInside.EndUpdate();
        }

        private void AddNote(string text)
            => lvInside.Items.Add(new ListViewItem(text) { ForeColor = SystemColors.GrayText });

        // ---- the buttons that act on the selected projects -----------------------------------

        private List<Project> SelectedProjects()
            => lvProjecsFound.SelectedItems.Cast<ListViewItem>().Select(row => row.Tag).OfType<Project>().ToList();

        private void OpenInExplorer()
        {
            Project? project = lvProjecsFound.FocusedItem is { Selected: true, Tag: Project focused }
                ? focused
                : SelectedProjects().FirstOrDefault();

            if (project == null)
            {
                Tell("Select a project in the list first.");
                return;
            }

            if (!File.Exists(project.FilePath))
            {
                Tell("That file is no longer there:\n" + project.FilePath);
                return;
            }

            // Opens the folder with the file already selected in it.
            Process.Start("explorer.exe", "/select,\"" + project.FilePath + "\"");
        }

        // The text "Copy location" puts on the clipboard: each selected file's full path, one per line.
        internal static string LocationText(IEnumerable<string> paths) => string.Join(Environment.NewLine, paths);

        private void CopyLocation()
        {
            List<Project> projects = SelectedProjects();
            if (projects.Count == 0)
            {
                Tell("Select a project in the list first.");
                return;
            }

            try
            {
                Clipboard.SetText(LocationText(projects.Select(p => p.FilePath)));
                ShowStatus(projects.Count == 1
                    ? "location copied to the clipboard"
                    : $"{projects.Count:N0} locations copied to the clipboard");
            }
            catch (System.Runtime.InteropServices.ExternalException)
            {
                Tell("The clipboard is busy. Try again in a moment.");
            }
        }

        private async Task CopyOrMoveAsync(bool move)
        {
            List<Project> projects = SelectedProjects();
            if (projects.Count == 0)
            {
                Tell("Select one or more projects in the list first.");
                return;
            }

            string verb = move ? "Move" : "Copy";
            using var picker = new FolderBrowserDialog
            {
                Description = projects.Count == 1 ? $"{verb} the selected project to" : $"{verb} the {projects.Count} selected projects to",
                UseDescriptionForTitle = true
            };
            if (picker.ShowDialog(this) != DialogResult.OK) return;

            string folder = picker.SelectedPath;
            List<string> files = projects.Select(p => p.FilePath).ToList();
            TransferReport report = await Task.Run(() => TransferFiles(files, folder, move));

            // A moved project now lives somewhere else: its row has to say so.
            if (move)
            {
                foreach (Project project in projects)
                {
                    if (!report.NewPaths.TryGetValue(project.FilePath, out string? newPath) || project.Row == null) continue;

                    project.FilePath = newPath;
                    project.Row.Text = Path.GetFileName(newPath);
                    project.Row.ToolTipText = newPath;
                    project.Row.SubItems[1].Text = Path.GetDirectoryName(newPath) ?? string.Empty;
                }
            }

            string done = move ? "moved" : "copied";
            ShowStatus(report.Done == 1 ? $"1 file {done} to {folder}" : $"{report.Done:N0} files {done} to {folder}");

            // Only something that needs a look gets a box: a file given a new name, or one left behind.
            var notes = new List<string>();
            if (report.Renamed > 0)
                notes.Add($"{report.Renamed} were given a new name because a file of that name was already there.");
            if (report.AlreadyThere > 0)
                notes.Add($"{report.AlreadyThere} were already in that folder and were left alone.");
            if (report.Failed.Count > 0)
                notes.Add($"Not {done}:\n" + string.Join("\n", report.Failed.Take(8)) + (report.Failed.Count > 8 ? "\n..." : string.Empty));
            if (notes.Count > 0) Tell(string.Join("\n\n", notes));
        }

        /// <summary>What a copy or move did. NewPaths maps each file that went to where it now is.</summary>
        internal sealed record TransferReport(int Done, int Renamed, int AlreadyThere, List<string> Failed, Dictionary<string, string> NewPaths);

        // Copies or moves the files into the folder. Nothing already there is ever overwritten: a
        // file whose name is taken goes in as "name (2)", "name (3)" and so on. A file being
        // moved into the folder it is already in is left alone.
        internal static TransferReport TransferFiles(IReadOnlyList<string> files, string folder, bool move)
        {
            int done = 0, renamed = 0, alreadyThere = 0;
            var failed = new List<string>();
            var newPaths = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            string targetFolder = Path.GetFullPath(folder);

            foreach (string file in files.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                try
                {
                    string? from = Path.GetDirectoryName(Path.GetFullPath(file));
                    if (move && string.Equals(from?.TrimEnd('\\'), targetFolder.TrimEnd('\\'), StringComparison.OrdinalIgnoreCase))
                    {
                        alreadyThere++;
                        continue;
                    }

                    string name = Path.GetFileNameWithoutExtension(file), extension = Path.GetExtension(file);
                    string target = Path.Combine(targetFolder, name + extension);
                    bool nameTaken = File.Exists(target);
                    for (int n = 2; File.Exists(target); n++)
                        target = Path.Combine(targetFolder, $"{name} ({n}){extension}");

                    if (move) File.Move(file, target, overwrite: false);
                    else File.Copy(file, target, overwrite: false);

                    done++;
                    if (nameTaken) renamed++;
                    newPaths[file] = target;
                }
                catch (Exception ex)
                {
                    failed.Add(Path.GetFileName(file) + " - " + ex.Message);
                }
            }

            return new TransferReport(done, renamed, alreadyThere, failed, newPaths);
        }

        // The same as double-clicking each selected file in Explorer.
        private void RunWithDefaultApp()
        {
            List<Project> projects = SelectedProjects();
            if (projects.Count == 0)
            {
                Tell("Select a project in the list first.");
                return;
            }

            if (projects.Count > 5 &&
                MessageBox.Show(this, $"Open all {projects.Count} selected files?", AppName,
                    MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK)
                return;

            foreach (Project project in projects)
            {
                try
                {
                    Process.Start(new ProcessStartInfo(project.FilePath) { UseShellExecute = true });
                }
                catch (Exception ex) when (ex is Win32Exception or FileNotFoundException or InvalidOperationException)
                {
                    Tell("Could not open\n" + project.FilePath + "\n\n" + ex.Message);
                    return;
                }
            }
        }

        private void Tell(string text)
            => MessageBox.Show(this, text, AppName, MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
}
