using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Windows.Forms;


namespace CampusLostFoundSystem
{
    public partial class Form1 : Form
    {
        string currentPage = "Dashboard";

        Panel appContainer, sidebar, mainArea, contentPanel, topPanel;
        DataGridView gridItems;
        TextBox txtSearch;
        Button btnSearch, btnAddSample;
        Label dashboardTitle;
        UserAccount loggedInUser;

        readonly Color Burgundy = Color.FromArgb(139, 0, 40);
        readonly Color BurgundyHover = Color.FromArgb(160, 30, 60);
        readonly Color BurgundyLight = Color.FromArgb(255, 235, 242);

        readonly Color PageBg = Color.FromArgb(246, 247, 252);
        readonly Color CardBg = Color.White;
        readonly Color SidebarBg = Color.FromArgb(250, 250, 253);

        readonly Color TextDark = Color.FromArgb(32, 33, 55);
        readonly Color TextMuted = Color.FromArgb(130, 130, 145);
        readonly Color BorderSoft = Color.FromArgb(225, 228, 235);

        public Form1()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            DatabaseHelper.CreateDefaultAdmin();
            ShowLoginPage();
        }

        private void Form1_Load(object sender, EventArgs e) { }

        Button CreatePrimaryButton(string text, int x, int y, int width, int height)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Location = new Point(x, y);
            btn.Size = new Size(width, height);
            btn.BackColor = Burgundy;
            btn.ForeColor = Color.White;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            btn.Cursor = Cursors.Hand;
            btn.UseVisualStyleBackColor = false;

            btn.Region = new Region(GetRoundedRect(new Rectangle(0, 0, width, height), 18));

            btn.MouseEnter += (s, e) => btn.BackColor = BurgundyHover;
            btn.MouseLeave += (s, e) => btn.BackColor = Burgundy;

            return btn;
        }

        TextBox CreateStyledTextBox(int x, int y, int width)
        {
            TextBox txt = new TextBox();
            txt.Location = new Point(x, y);
            txt.Size = new Size(width, 34);
            txt.Font = new Font("Segoe UI", 10);
            txt.BorderStyle = BorderStyle.FixedSingle;
            txt.BackColor = Color.White;
            txt.ForeColor = TextDark;

            return txt;
        }

        TextBox CreatePremiumInput(Panel parent, string label, string placeholder, int x, int y, int width, bool password = false)
        {
            Label lbl = new Label();
            lbl.Text = label;
            lbl.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            lbl.ForeColor = TextDark;
            lbl.Location = new Point(x, y);
            lbl.Size = new Size(width, 20);
            parent.Controls.Add(lbl);

            Panel inputBox = new Panel();
            inputBox.Location = new Point(x, y + 25);
            inputBox.Size = new Size(width, 42);
            inputBox.BackColor = Color.FromArgb(250, 250, 252);
            parent.Controls.Add(inputBox);

            inputBox.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                Rectangle rect = new Rectangle(0, 0, inputBox.Width - 1, inputBox.Height - 1);

                using (GraphicsPath path = GetRoundedRect(rect, 14))
                using (Pen border = new Pen(Color.FromArgb(220, 224, 232), 1))
                {
                    inputBox.Region = new Region(path);
                    e.Graphics.DrawPath(border, path);
                }
            };

            TextBox txt = new TextBox();
            txt.BorderStyle = BorderStyle.None;
            txt.BackColor = inputBox.BackColor;
            txt.ForeColor = TextDark;
            txt.Font = new Font("Segoe UI", 10);
            txt.Location = new Point(14, 12);
            txt.Width = width - 28;

            txt.Text = placeholder;
            txt.ForeColor = Color.FromArgb(160, 165, 180);

            txt.Enter += (s, e) =>
            {
                if (txt.Text == placeholder)
                {
                    txt.Text = "";

                    txt.ForeColor = TextDark;

                    if (password)
                        txt.PasswordChar = '●';
                }
            };

            txt.Leave += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txt.Text))
                {
                    txt.PasswordChar = '\0';
                    txt.Text = placeholder;
                    txt.ForeColor = Color.FromArgb(160, 165, 180);
                }
            };

            if (password)
                txt.PasswordChar = '●';

            inputBox.Controls.Add(txt);
            inputBox.Click += (s, e) => txt.Focus();
            return txt;
        }

        void StyleGrid(DataGridView grid)
        {
            grid.BackgroundColor = Color.White;
            grid.BorderStyle = BorderStyle.None;
            grid.ReadOnly = true;
            grid.AllowUserToAddRows = false;
            grid.RowHeadersVisible = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            grid.Font = new Font("Segoe UI", 9);
            grid.GridColor = Color.FromArgb(238, 240, 245);

            grid.EnableHeadersVisualStyles = false;
            grid.ColumnHeadersHeight = 42;
            grid.RowTemplate.Height = 58;

            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(248, 249, 252);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = TextDark;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            grid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

            grid.DefaultCellStyle.BackColor = Color.White;
            grid.DefaultCellStyle.ForeColor = TextDark;
            grid.DefaultCellStyle.SelectionBackColor = BurgundyLight;
            grid.DefaultCellStyle.SelectionForeColor = TextDark;

            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(252, 252, 254);

            grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
        }

        Panel CreateCard(int x, int y, int width, int height)
        {
            Panel card = new Panel();
            card.Location = new Point(x, y);
            card.Size = new Size(width, height);
            card.BackColor = CardBg;

            card.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                Rectangle rect = new Rectangle(0, 0, card.Width - 1, card.Height - 1);

                using (GraphicsPath path = GetRoundedRect(rect, 22))
                using (Pen border = new Pen(Color.FromArgb(210, 215, 225), 1))
                {
                    card.Region = new Region(path);
                    e.Graphics.DrawPath(border, path);
                }
            };

            return card;
        }

        void ShowLoginPage()
        {
            ShowAuthPage(false);
        }

        void ShowRegisterPage()
        {
            ShowAuthPage(true);
        }

        void ShowAuthPage(bool registerMode = false)
        {
            Controls.Clear();

            Text = registerMode
                ? "Campus Lost & Found System - Register"
                : "Campus Lost & Found System - Login";

            WindowState = FormWindowState.Maximized;
            BackColor = Color.FromArgb(245, 247, 252);

            Panel card = CreateCard(0, 0, 900, 520);
            card.Location = new Point(
                (ClientSize.Width - card.Width) / 2,
                (ClientSize.Height - card.Height) / 2
            );
            Controls.Add(card);

            Resize += (s, e) =>
            {
                card.Location = new Point(
                    (ClientSize.Width - card.Width) / 2,
                    (ClientSize.Height - card.Height) / 2
                );
            };

            Panel slidePanel = new Panel();
            slidePanel.Size = new Size(430, 520);
            slidePanel.Location = registerMode ? new Point(470, 0) : new Point(0, 0);
            slidePanel.BackColor = Burgundy;
            card.Controls.Add(slidePanel);

            Label appTitle = new Label();
            appTitle.Text = "Campus Lost and Found";
            appTitle.Font = new Font("Segoe UI", 15, FontStyle.Bold);
            appTitle.ForeColor = Color.FromArgb(255, 240, 245);
            appTitle.AutoSize = true;
            appTitle.Location = new Point(40, 35);

            slidePanel.Controls.Add(appTitle);

            Panel formPanel = new Panel();
            formPanel.Size = new Size(430, 520);
            formPanel.Location = registerMode ? new Point(0, 0) : new Point(470, 0);
            formPanel.BackColor = Color.White;
            card.Controls.Add(formPanel);

        

            Label panelTitle = new Label();
            panelTitle.Text = registerMode ? "WELCOME BACK" : "WELCOME";
            panelTitle.Font = new Font("Segoe UI", 25, FontStyle.Bold);
            panelTitle.ForeColor = Color.White;
            panelTitle.Location = new Point(55, 155);
            panelTitle.Size = new Size(330, 55);
            slidePanel.Controls.Add(panelTitle);

            Label panelSub = new Label();
            panelSub.Text = registerMode
                ? "Already have an account? Log in to continue managing campus reports."
                : "Securely report, browse, and claim lost or found items within the campus.";

            panelSub.Font = new Font("Segoe UI", 10);
            panelSub.ForeColor = Color.FromArgb(245, 245, 245);
            panelSub.Location = new Point(60, 220);
            panelSub.Size = new Size(295, 80);
            slidePanel.Controls.Add(panelSub);

            Button switchButton = new Button();
            switchButton.Text = registerMode ? "Log in" : "Create Account";
            switchButton.Location = new Point(60, 325);
            switchButton.Size = new Size(190, 42);
            switchButton.FlatStyle = FlatStyle.Flat;
            switchButton.FlatAppearance.BorderSize = 0;
            switchButton.BackColor = Color.White;
            switchButton.ForeColor = Burgundy;
            switchButton.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            switchButton.Cursor = Cursors.Hand;

            slidePanel.Controls.Add(switchButton);

            switchButton.Paint += (s, e) =>
            {
                switchButton.Region = new Region(GetRoundedRect(switchButton.ClientRectangle, 18));
            };

            int formX = 55;

            Label title = new Label();
            title.Text = registerMode ? "Create Account" : "Log in";
            title.Font = new Font("Segoe UI", 26, FontStyle.Bold);
            title.ForeColor = TextDark;
            title.Location = new Point(formX, registerMode ? 55 : 80);
            title.Size = new Size(330, 50);
            formPanel.Controls.Add(title);

            Label sub = new Label();
            sub.Text = registerMode
                ? "Register your account to continue"
                : "Login to access your account";

            sub.Font = new Font("Segoe UI", 10);
            sub.ForeColor = TextMuted;
            sub.Location = new Point(formX + 5, registerMode ? 105 : 130);
            sub.Size = new Size(330, 25);
            formPanel.Controls.Add(sub);

            TextBox txtName = null;

            int startY = registerMode ? 150 : 180;

            if (registerMode)
            {
                
                txtName = CreatePremiumInput(formPanel, "Full Name", "Enter your full name", formX + 5, startY, 300);

                startY += 70;
            }

            TextBox txtUsername = CreatePremiumInput(formPanel, "Username", "Enter your username", formX + 5, startY, 300);

            TextBox txtPassword = CreatePremiumInput(formPanel, "Password", "Enter your password", formX + 5, startY + 70, 300, true);

            Button mainButton = CreatePrimaryButton(
                registerMode ? "Create Account" : "Log in",
                formX + 5,
                startY + 160,
                300,
                42
            );

            formPanel.Controls.Add(mainButton);

            mainButton.Click += (s, e) =>
            {
                if (registerMode)
                {
                    if (txtName.Text.Trim() == "" ||
                        txtUsername.Text.Trim() == "" ||
                        txtPassword.Text.Trim() == "")
                    {
                        MessageBox.Show("Please complete all fields.");
                        return;
                    }

                    DatabaseHelper.AddUser(new UserAccount
                    {
                        FullName = txtName.Text,
                        Username = txtUsername.Text,
                        Password = txtPassword.Text,
                        Role = "User"
                    });

                    ShowAuthPage(false);
                    ShowSuccess("Account created successfully.");
                }
                else
                {
                    var user = DatabaseHelper.LoginUser(txtUsername.Text, txtPassword.Text);

                    if (user == null)
                    {
                        ShowError("Invalid username or password.");
                        return;
                    }

                    loggedInUser = user;
                    BuildUI();
                    LoadData();
                }
                formPanel.BringToFront();
            };

            switchButton.Click += (s, e) =>
            {
                int slideTarget = registerMode ? 0 : 470;
                int formTarget = registerMode ? 470 : 0;

                Timer timer = new Timer();
                timer.Interval = 8;

                timer.Tick += (a, b) =>
                {
                    if (!registerMode)
                    {
                        slidePanel.Left += 20;
                        formPanel.Left -= 20;

                        if (slidePanel.Left >= slideTarget)
                        {
                            timer.Stop();
                            timer.Dispose();
                            ShowAuthPage(true);
                        }
                    }
                    else
                    {
                        slidePanel.Left -= 20;
                        formPanel.Left += 20;

                        if (slidePanel.Left <= slideTarget)
                        {
                            timer.Stop();
                            timer.Dispose();
                            ShowAuthPage(false);
                        }
                    }
                };

                timer.Start();
            };
        }

        void BuildUI()
        {
            Controls.Clear();

            Text = "Campus Lost & Found System";
            WindowState = FormWindowState.Maximized;
            BackColor = PageBg;
            Font = new Font("Segoe UI", 10);

            appContainer = new Panel();
            appContainer.Dock = DockStyle.Fill;
            appContainer.BackColor = PageBg;
            Controls.Add(appContainer);

            sidebar = new Panel();
            sidebar.Location = new Point(20, 20);
            sidebar.Size = new Size(245, appContainer.Height - 40);
            sidebar.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            sidebar.BackColor = CardBg;
            appContainer.Controls.Add(sidebar);

            sidebar.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                sidebar.Region = new Region(GetRoundedRect(sidebar.ClientRectangle, 24));
            };

            mainArea = new Panel();
            mainArea.Location = new Point(285, 20);
            mainArea.Size = new Size(appContainer.Width - 305, appContainer.Height - 40);
            mainArea.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            mainArea.BackColor = PageBg;
            appContainer.Controls.Add(mainArea);

            contentPanel = new Panel();
            contentPanel.Dock = DockStyle.Fill;
            contentPanel.BackColor = PageBg;
            mainArea.Controls.Add(contentPanel);

            topPanel = new Panel();
            topPanel.Dock = DockStyle.Top;
            topPanel.Height = 90;
            topPanel.BackColor = PageBg;
            mainArea.Controls.Add(topPanel);

            topPanel.BringToFront();

            BuildSidebar();
            BuildTopBar();
            BuildDashboard();
        }

        void BuildSidebar()
        {
            Label logo = new Label();
            logo.Text = "◈ Campus L&F";
            logo.Font = new Font("Segoe UI", 15, FontStyle.Bold);
            logo.ForeColor = Burgundy;
            logo.Location = new Point(28, 28);
            logo.Size = new Size(190, 35);
            sidebar.Controls.Add(logo);

            Label menuLabel = new Label();
            menuLabel.Text = "MAIN MENU";
            menuLabel.Font = new Font("Segoe UI", 8, FontStyle.Bold);
            menuLabel.ForeColor = TextMuted;
            menuLabel.Location = new Point(32, 88);
            menuLabel.Size = new Size(120, 20);
            sidebar.Controls.Add(menuLabel);

            AddMenu("▦  Dashboard", 118, false);
            AddMenu("＋  Report Item", 168, false);
            AddMenu("⌕  Lost Items", 218, false);
            AddMenu("□  Found Items", 268, false);

            if (loggedInUser != null && loggedInUser.Role == "Admin")
            {
                AddMenu("✓  Claim Requests", 318, false);
                AddMenu("🕘  Activity Logs", 368, false);
                AddMenu("🕘  Activity Logs", 368, false);
                AddMenu("⚙  Admin Panel", 418, false);
            }

            Panel userCard = new Panel();
            userCard.Location = new Point(20, sidebar.Height - 135);
            userCard.Size = new Size(205, 70);
            userCard.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            userCard.BackColor = BurgundyLight;
            sidebar.Controls.Add(userCard);

            userCard.Paint += (s, e) =>
            {
                userCard.Region = new Region(GetRoundedRect(userCard.ClientRectangle, 18));
            };

            Label userName = new Label();
            userName.Text = loggedInUser?.FullName ?? "User";
            userName.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            userName.ForeColor = TextDark;
            userName.Location = new Point(15, 12);
            userName.Size = new Size(170, 22);
            userCard.Controls.Add(userName);

            Label userRole = new Label();
            userRole.Text = loggedInUser?.Role ?? "User";
            userRole.Font = new Font("Segoe UI", 8);
            userRole.ForeColor = Burgundy;
            userRole.Location = new Point(15, 36);
            userRole.Size = new Size(170, 22);
            userCard.Controls.Add(userRole);

            Button btnLogout = new Button();
            btnLogout.Text = "⎋  Logout";
            btnLogout.Location = new Point(20, sidebar.Height - 55);
            btnLogout.Size = new Size(205, 38);
            btnLogout.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            btnLogout.FlatStyle = FlatStyle.Flat;
            btnLogout.FlatAppearance.BorderSize = 0;
            btnLogout.BackColor = CardBg;
            btnLogout.ForeColor = Burgundy;
            btnLogout.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            btnLogout.TextAlign = ContentAlignment.MiddleLeft;
            btnLogout.Cursor = Cursors.Hand;

            btnLogout.Click += (s, e) =>
            {
                loggedInUser = null;
                gridItems = null;
                ShowLoginPage();
            };

            sidebar.Controls.Add(btnLogout);
        }

        void AddMenu(string text, int top, bool active)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Location = new Point(20, top);
            btn.Size = new Size(205, 42);
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.TextAlign = ContentAlignment.MiddleLeft;
            btn.Cursor = Cursors.Hand;

            bool isActive = text.Contains(currentPage);

            btn.BackColor = isActive ? BurgundyLight : CardBg;
            btn.ForeColor = isActive ? Burgundy : TextMuted;
            btn.Font = new Font("Segoe UI", 10, isActive ? FontStyle.Bold : FontStyle.Regular);

            btn.MouseEnter += (s, e) =>
            {
                if (!isActive)
                    btn.BackColor = Color.FromArgb(248, 240, 244);
            };

            btn.MouseLeave += (s, e) =>
            {
                if (!isActive)
                    btn.BackColor = CardBg;
            };

            btn.Paint += (s, e) =>
            {
                btn.Region = new Region(GetRoundedRect(btn.ClientRectangle, 14));
            };

            btn.Click += (s, e) =>
            {
                if (text.Contains("Dashboard"))
                {
                    currentPage = "Dashboard";
                    RefreshSidebar();
                    ShowDashboardPage();
                }
                else if (text.Contains("Report Item"))
                {
                    currentPage = "Report Item";
                    RefreshSidebar();
                    ShowReportItemPage();
                }
                else if (text.Contains("Lost Items"))
                {
                    currentPage = "Lost Items";
                    RefreshSidebar();
                    ShowFilteredItemsPage("Lost");
                }
                else if (text.Contains("Found Items"))
                {
                    currentPage = "Found Items";
                    RefreshSidebar();
                    ShowFilteredItemsPage("Found");
                }
                else if (text.Contains("Claim Requests"))
                {
                    currentPage = "Claim Requests";
                    RefreshSidebar();
                    ShowClaimRequestsPage();
                }
                else if (text.Contains("Activity Logs"))
                {
                    currentPage = "Activity Logs";
                    RefreshSidebar();
                    ShowActivityLogsPage();
                }
                else if (text.Contains("Admin Panel"))
                {
                    currentPage = "Admin Panel";
                    RefreshSidebar();
                    ShowAdminPage();
                }
            };

            btn.MouseEnter += (s, e) =>
            {
                if (currentPage != text)
                    btn.BackColor = Color.FromArgb(245, 238, 242);
            };

            btn.MouseLeave += (s, e) =>
            {
                if (currentPage != text)
                    btn.BackColor = Color.Transparent;
            };

            sidebar.Controls.Add(btn);
        }

        void RefreshSidebar()
        {
            sidebar.Controls.Clear();
            BuildSidebar();
        }

        void BuildTopBar()
        {
            dashboardTitle = new Label();
            dashboardTitle.Text = currentPage;
            dashboardTitle.Font = new Font("Segoe UI", 24, FontStyle.Bold);
            dashboardTitle.ForeColor = TextDark;
            dashboardTitle.Location = new Point(5, 12);
            dashboardTitle.Size = new Size(350, 45);
            topPanel.Controls.Add(dashboardTitle);

            Label subtitle = new Label();
            subtitle.Text = "Manage campus lost and found reports";
            subtitle.Font = new Font("Segoe UI", 9);
            subtitle.ForeColor = TextMuted;
            subtitle.Location = new Point(8, 58);
            subtitle.Size = new Size(350, 22);
            topPanel.Controls.Add(subtitle);

            Panel searchContainer = new Panel();
            searchContainer.Location = new Point(topPanel.Width - 520, 24);
            searchContainer.Size = new Size(380, 42);
            searchContainer.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            searchContainer.BackColor = Color.White;
            topPanel.Controls.Add(searchContainer);

            searchContainer.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                Rectangle rect = new Rectangle(0, 0, searchContainer.Width - 1, searchContainer.Height - 1);

                using (GraphicsPath path = GetRoundedRect(rect, 18))
                using (Pen border = new Pen(Color.FromArgb(215, 220, 230), 1))
                {
                    searchContainer.Region = new Region(path);
                    e.Graphics.DrawPath(border, path);
                }
            };

            Label searchIcon = new Label();
            searchIcon.Text = "⌕";
            searchIcon.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            searchIcon.ForeColor = TextMuted;
            searchIcon.Location = new Point(14, 10);
            searchIcon.Size = new Size(25, 25);
            searchContainer.Controls.Add(searchIcon);

            txtSearch = new TextBox();
            txtSearch.Text = "Search item, category, or location...";
            txtSearch.ForeColor = Color.Gray;
            txtSearch.BorderStyle = BorderStyle.None;
            txtSearch.BackColor = Color.White;
            txtSearch.Font = new Font("Segoe UI", 10);
            txtSearch.Location = new Point(45, 12);
            txtSearch.Width = 310;
            txtSearch.Enter += RemovePlaceholder;
            txtSearch.Leave += AddPlaceholder;
            txtSearch.TextChanged += TxtSearch_TextChanged;
            searchContainer.Controls.Add(txtSearch);

            btnSearch = CreatePrimaryButton("Search", topPanel.Width - 125, 24, 115, 42);
            btnSearch.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            btnSearch.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            topPanel.Controls.Add(btnSearch);
        }

        void BuildDashboard()
        {
            contentPanel.Controls.Clear();

            Label overview = new Label();
            overview.Text = "Overview";
            overview.Font = new Font("Segoe UI", 13, FontStyle.Bold);
            overview.ForeColor = Color.FromArgb(45, 45, 70);
            overview.Location = new Point(40, 25);
            overview.Size = new Size(200, 30);
            contentPanel.Controls.Add(overview);

            AddCard("Total Lost", CountItems("Lost").ToString(), "Reported lost items", 10, 95);
            AddCard("Total Reports", CountAllItems().ToString(), "All reported items", 245, 95);
            AddCard("Pending Claims", CountClaims("Not Claimed").ToString(), "Waiting approval", 480, 95);
            AddCard("Returned", CountClaims("Claimed").ToString(), "Successfully claimed", 715, 95);

            AddCard("Top Category", GetMostCommonCategory(), "Most reported category", 950, 95);
            AddCard("Latest Report", GetLatestReport(), "Most recent item", 1185, 95);
            AddCard("Claim Rate", GetClaimRate(), "Successful claims", 1420, 95);

            Panel tableCard = CreateCard(10, 225, contentPanel.Width - 20, contentPanel.Height - 255);
            tableCard.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            contentPanel.Controls.Add(tableCard);
            SlideIn(tableCard);

            Label reportsTitle = new Label();
            reportsTitle.Text = "Recent Reports";
            reportsTitle.Font = new Font("Segoe UI", 15, FontStyle.Bold);
            reportsTitle.ForeColor = TextDark;
            reportsTitle.Location = new Point(30, 25);
            reportsTitle.Size = new Size(260, 30);
            tableCard.Controls.Add(reportsTitle);

            Label reportsSub = new Label();
            reportsSub.Text = "All reported lost and found items";
            reportsSub.Font = new Font("Segoe UI", 9);
            reportsSub.ForeColor = Color.Gray;
            reportsSub.Location = new Point(30, 55);
            reportsSub.Size = new Size(400, 25);
            tableCard.Controls.Add(reportsSub);

        

            gridItems = CreateReportsGrid();
            gridItems.Location = new Point(30, 90);
            gridItems.Size = new Size(tableCard.Width - 60, tableCard.Height - 120);
            gridItems.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            gridItems.CellClick += GridItems_CellClick;
            tableCard.Controls.Add(gridItems);
            Label emptyReports = new Label();
            emptyReports.Text = "No reports yet.\nUse the Report Item page to add a lost or found item.";
            emptyReports.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            emptyReports.ForeColor = TextMuted;
            emptyReports.TextAlign = ContentAlignment.MiddleCenter;
            emptyReports.Location = new Point(0, 170);
            emptyReports.Size = new Size(tableCard.Width, 80);
            emptyReports.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            emptyReports.Visible = CountAllItems() == 0;
            tableCard.Controls.Add(emptyReports);
            emptyReports.BringToFront();
        }

        DataGridView CreateReportsGrid()
        {
            DataGridView grid = new DataGridView();
            StyleGrid(grid);
            grid.AutoGenerateColumns = false;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;

            grid.Columns.Add("Id", "Id");
            grid.Columns["Id"].Visible = false;
            grid.Columns.Add("No", "No.");
            DataGridViewImageColumn imgCol = new DataGridViewImageColumn();
            imgCol.Name = "Image";
            imgCol.HeaderText = "Image";
            imgCol.ImageLayout = DataGridViewImageCellLayout.Zoom;

            grid.Columns.Add(imgCol);
            grid.Columns.Add("ItemName", "Item Name");
            grid.Columns.Add("Category", "Category");
            grid.Columns.Add("DateReported", "Date Reported");
            grid.Columns.Add("Status", "Status");
            grid.Columns.Add("Location", "Location");
            grid.Columns.Add("Description", "Description");
            grid.Columns.Add("Reporter", "Reporter");
            grid.Columns.Add("ClaimStatus", "Claim Status");
            grid.Columns.Add("Edit", "Edit");
            grid.Columns.Add("Delete", "Delete");

            grid.Columns["No"].Width = 50;
            grid.Columns["No"].Width = 90;
            grid.Columns["ItemName"].Width = 150;
            grid.Columns["Category"].Width = 150;
            grid.Columns["DateReported"].Width = 170;
            grid.Columns["Status"].Width = 90;
            grid.Columns["Location"].Width = 120;
            grid.Columns["Description"].Width = 220;
            grid.Columns["Reporter"].Width = 120;
            grid.Columns["ClaimStatus"].Width = 130;
            grid.Columns["Edit"].Width = 80;
            grid.Columns["Delete"].Width = 80;

            grid.Columns["No"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            grid.Columns["No"].DisplayIndex = 0;
            grid.Columns["Image"].DisplayIndex = 1;
            grid.Columns["ItemName"].DisplayIndex = 2;
            grid.Columns["Category"].DisplayIndex = 3;
            grid.Columns["DateReported"].DisplayIndex = 4;
            grid.CellFormatting += (s, e) =>
            {
                if (e.RowIndex < 0 || e.Value == null) return;

                string col = grid.Columns[e.ColumnIndex].Name;

                if (col == "Status")
                {
                    if (e.Value.ToString() == "Lost")
                    {
                        e.CellStyle.ForeColor = Color.White;
                        e.CellStyle.BackColor = Color.FromArgb(180, 40, 40);
                    }
                    else if (e.Value.ToString() == "Found")
                    {
                        e.CellStyle.ForeColor = Color.White;
                        e.CellStyle.BackColor = Color.FromArgb(40, 140, 80);
                    }
                }

                if (col == "ClaimStatus")
                {
                    if (e.Value.ToString() == "Claimed")
                    {
                        e.CellStyle.ForeColor = Color.White;
                        e.CellStyle.BackColor = Color.FromArgb(40, 140, 80);
                    }
                    else if (e.Value.ToString() == "Not Claimed")
                    {
                        e.CellStyle.ForeColor = Color.White;
                        e.CellStyle.BackColor = Burgundy;
                    }
                }
            };

            return grid;
        }

        void AddCard(string title, string number, string desc, int x, int y)
        {
            Panel card = CreateCard(x, y, 205, 90);
            contentPanel.Controls.Add(card);

            Panel accent = new Panel();
            accent.Location = new Point(0, 0);
            accent.Size = new Size(5, card.Height);
            accent.BackColor = Burgundy;
            card.Controls.Add(accent);

            Label lblTitle = new Label();
            lblTitle.Text = title;
            lblTitle.ForeColor = TextMuted;
            lblTitle.Font = new Font("Segoe UI", 8, FontStyle.Bold);
            lblTitle.Location = new Point(20, 14);
            lblTitle.Size = new Size(175, 18);
            card.Controls.Add(lblTitle);

            Label lblNumber = new Label();
            lblNumber.Text = number;
            lblNumber.Font = new Font("Segoe UI", 22, FontStyle.Bold);
            lblNumber.ForeColor = TextDark;
            lblNumber.Location = new Point(20, 32);
            lblNumber.Size = new Size(180, 40);
            card.Controls.Add(lblNumber);

            Label lblDesc = new Label();
            lblDesc.Text = desc;
            lblDesc.Font = new Font("Segoe UI", 8);
            lblDesc.ForeColor = TextMuted;
            lblDesc.Location = new Point(20, 72);
            lblDesc.Size = new Size(180, 18);
            card.Controls.Add(lblDesc);
        }

        void LoadData()
        {
            if (gridItems == null) return;

            string search = txtSearch.Text == "Search item, category, or location..." ? "" : txtSearch.Text;
            var data = DatabaseHelper.GetItems(search);

            gridItems.Rows.Clear();

            for (int i = 0; i < data.Count; i++)
            {
                var item = data[i];

                Image img = null;

                if (!string.IsNullOrEmpty(item.ImagePath) && File.Exists(item.ImagePath))
                {
                    using (var temp = new Bitmap(item.ImagePath))
                    {
                        img = new Bitmap(temp);
                    }
                }
                
                gridItems.Rows.Add(
                    item.Id,
                    i + 1,
                    img,
                    item.ItemName,
                    item.Category,
                    item.DateReported.ToString("MMM dd, yyyy hh:mm tt"),
                    item.Status,
                    item.Location,
                    item.Description,
                    item.Reporter,
                    item.ClaimStatus,
                    CanManageItem(item) ? "✎ Edit" : "",
                    CanManageItem(item) ? "🗑 Delete" : ""
                );
            }

            gridItems.ClearSelection();

            gridItems.Columns["Edit"].DefaultCellStyle.BackColor = Color.FromArgb(235, 245, 255);
            gridItems.Columns["Edit"].DefaultCellStyle.ForeColor = Color.FromArgb(30, 90, 180);

            gridItems.Columns["Delete"].DefaultCellStyle.BackColor = Color.FromArgb(255, 235, 238);
            gridItems.Columns["Delete"].DefaultCellStyle.ForeColor = Color.FromArgb(180, 40, 40);

            gridItems.Columns["Edit"].DefaultCellStyle.Alignment =
                DataGridViewContentAlignment.MiddleCenter;

            gridItems.Columns["Delete"].DefaultCellStyle.Alignment =
                DataGridViewContentAlignment.MiddleCenter;

            if (gridItems.Columns.Count > 1)
            {
                gridItems.FirstDisplayedScrollingColumnIndex = 1;
                gridItems.HorizontalScrollingOffset = 0;
            }
        }

        void ShowDashboardPage()
        {
            dashboardTitle.Text = "Dashboard";
            ClearContent();
            BuildDashboard();
            LoadData();
        }

        void ShowReportItemPage()
        {
            dashboardTitle.Text = "Report Items";
            ClearContent();

            Panel formCard = CreateCard(
                10,
                80,
                contentPanel.Width - 20,
                contentPanel.Height - 40
            );

            formCard.Anchor =
                AnchorStyles.Top |
                AnchorStyles.Bottom |
                AnchorStyles.Left |
                AnchorStyles.Right;

            contentPanel.Controls.Add(formCard);
            SlideIn(formCard);



            Panel detailsCard = CreateCard(80, 35, 610, 560);
            formCard.Controls.Add(detailsCard);

            Label detailsTitle = new Label();
            detailsTitle.Text = "Item Details";
            detailsTitle.Font = new Font("Segoe UI", 13, FontStyle.Bold);
            detailsTitle.ForeColor = TextDark;
            detailsTitle.Location = new Point(30, 25);
            detailsTitle.AutoSize = true;
            detailsCard.Controls.Add(detailsTitle);

            TextBox txtName = AddLabeledInput(
                detailsCard,
                "Item Name",
                "Ex: Black Tumbler",
                30,
                75
            );

            TextBox txtCategory = AddLabeledInput(
                detailsCard,
                "Category",
                "Ex: Electronics",
                30,
                145
            );

            TextBox txtLocation = AddLabeledInput(
                detailsCard,
                "Location",
                "Ex: Library 2nd Floor, Main Campus",
                30,
                215
            );

            TextBox txtDescription = AddLabeledInput(
                detailsCard,
                "Description",
                "Describe the item details...",
                30,
                285
            );

            TextBox txtReporter = AddLabeledInput(
                detailsCard,
                "Reporter Name",
                "Enter your Full name or User name",
                30,
                355
            );

            Label lblStatus = new Label();
            lblStatus.Text = "Status";
            lblStatus.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            lblStatus.ForeColor = TextDark;
            lblStatus.Location = new Point(30, 425);
            lblStatus.Size = new Size(200, 20);
            detailsCard.Controls.Add(lblStatus);

            string selectedStatus = "Lost";

            Button btnLost = new Button();
            btnLost.Text = "Lost";
            btnLost.Location = new Point(30, 455);
            btnLost.Size = new Size(260, 42);
            btnLost.FlatStyle = FlatStyle.Flat;
            btnLost.FlatAppearance.BorderSize = 0;
            btnLost.BackColor = Burgundy;
            btnLost.ForeColor = Color.White;
            btnLost.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            btnLost.Cursor = Cursors.Hand;
            detailsCard.Controls.Add(btnLost);

            btnLost.Paint += (s, e) =>
            {
                btnLost.Region = new Region(GetRoundedRect(btnLost.ClientRectangle, 16));
            };

            Button btnFound = new Button();
            btnFound.Text = "Found";
            btnFound.Location = new Point(310, 455);
            btnFound.Size = new Size(260, 42);
            btnFound.FlatStyle = FlatStyle.Flat;
            btnFound.FlatAppearance.BorderSize = 0;
            btnFound.BackColor = BurgundyLight;
            btnFound.ForeColor = Burgundy;
            btnFound.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            btnFound.Cursor = Cursors.Hand;
            detailsCard.Controls.Add(btnFound);

            btnFound.Paint += (s, e) =>
            {
                btnFound.Region = new Region(GetRoundedRect(btnFound.ClientRectangle, 16));
            };

            btnLost.Click += (s, e) =>
            {
                selectedStatus = "Lost";

                btnLost.BackColor = Burgundy;
                btnLost.ForeColor = Color.White;

                btnFound.BackColor = BurgundyLight;
                btnFound.ForeColor = Burgundy;
            };

            btnFound.Click += (s, e) =>
            {
                selectedStatus = "Found";

                btnFound.BackColor = Burgundy;
                btnFound.ForeColor = Color.White;

                btnLost.BackColor = BurgundyLight;
                btnLost.ForeColor = Burgundy;
            };



            Panel imageCard = CreateCard(730, 35, 420, 560);
            formCard.Controls.Add(imageCard);

            Label imageTitle = new Label();
            imageTitle.Text = "Item Photo";
            imageTitle.Font = new Font("Segoe UI", 13, FontStyle.Bold);
            imageTitle.ForeColor = TextDark;
            imageTitle.Location = new Point(30, 25);
            imageTitle.AutoSize = true;
            imageCard.Controls.Add(imageTitle);

            Label imageSub = new Label();
            imageSub.Text = "Upload a clear image of the item.";
            imageSub.Font = new Font("Segoe UI", 9);
            imageSub.ForeColor = TextMuted;
            imageSub.Location = new Point(32, 55);
            imageSub.AutoSize = true;
            imageCard.Controls.Add(imageSub);

            string selectedImagePath = "";

            PictureBox picPreview = new PictureBox();
            picPreview.Location = new Point(35, 105);
            picPreview.Size = new Size(350, 285);
            picPreview.BorderStyle = BorderStyle.FixedSingle;
            picPreview.SizeMode = PictureBoxSizeMode.Zoom;
            picPreview.BackColor = Color.FromArgb(248, 249, 252);
            imageCard.Controls.Add(picPreview);

            Label placeholder = new Label();
            placeholder.Text = "No image selected";
            placeholder.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            placeholder.ForeColor = TextMuted;
            placeholder.AutoSize = true;
            placeholder.Location = new Point(140, 235);
            imageCard.Controls.Add(placeholder);
            placeholder.BringToFront();

            Button btnUpload = CreatePrimaryButton("Upload Image", 35, 415, 165, 42);
            imageCard.Controls.Add(btnUpload);

            Button btnClear = new Button();
            btnClear.Text = "Clear";
            btnClear.Location = new Point(220, 415);
            btnClear.Size = new Size(165, 42);
            btnClear.FlatStyle = FlatStyle.Flat;
            btnClear.FlatAppearance.BorderSize = 0;
            btnClear.BackColor = BurgundyLight;
            btnClear.ForeColor = Burgundy;
            btnClear.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            btnClear.Cursor = Cursors.Hand;
            imageCard.Controls.Add(btnClear);

            btnClear.Paint += (s, e) =>
            {
                btnClear.Region = new Region(GetRoundedRect(btnClear.ClientRectangle, 16));
            };

            btnUpload.Click += (s, e) =>
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    selectedImagePath = ofd.FileName;

                    using (var temp = new Bitmap(selectedImagePath))
                    {
                        picPreview.Image = new Bitmap(temp);
                    }

                    placeholder.Visible = false;
                }
            };

            btnClear.Click += (s, e) =>
            {
                selectedImagePath = "";
                picPreview.Image = null;
                placeholder.Visible = true;
            };

            Button btnSave = CreatePrimaryButton("Save Report", 35, 485, 350, 46);
            imageCard.Controls.Add(btnSave);

            btnSave.Click += (s, e) =>
            {
                if (txtName.Text.Trim() == "" ||
                    txtCategory.Text.Trim() == "" ||
                    txtLocation.Text.Trim() == "")
                {
                    MessageBox.Show("Please complete item name, category, and location.");
                    return;
                }

                LostFoundItem item = new LostFoundItem
                {
                    ItemName = txtName.Text,
                    Category = txtCategory.Text,
                    Status = selectedStatus,
                    Location = txtLocation.Text,
                    Description = txtDescription.Text,
                    Reporter = loggedInUser?.FullName ?? txtReporter.Text,
                    ClaimStatus = "Not Claimed",
                    DateReported = DateTime.Now,
                    ImagePath = selectedImagePath
                };

                DatabaseHelper.AddItem(item);

                DatabaseHelper.AddLog(new ActivityLog
                {
                    Action = "Reported new item: " + item.ItemName,
                    PerformedBy = loggedInUser?.FullName ?? "Unknown",
                    Role = loggedInUser?.Role ?? "Unknown",
                    DateCreated = DateTime.Now
                });

                currentPage = "Dashboard";
                RefreshSidebar();
                ShowDashboardPage();

                ShowSuccess("Item report saved successfully.");
            };
        }

        TextBox AddLabeledInput(
                    Panel parent,
                    string label,
                    string placeholder,
                    int x,
                    int y
                )
        {
            return CreatePremiumInput(
                parent,
                label,
                placeholder,
                x,
                y,
                540
            );
        }

        void ShowFilteredItemsPage(string status)
        {
            dashboardTitle.Text = status + " Items";

            ClearContent();

            Panel tableCard = CreateCard(
                10,
                80,
                contentPanel.Width - 20,
                contentPanel.Height - 100
            );

            tableCard.Anchor =
                AnchorStyles.Top |
                AnchorStyles.Bottom |
                AnchorStyles.Left |
                AnchorStyles.Right;

            contentPanel.Controls.Add(tableCard);
            SlideIn(tableCard);

            Label title = new Label();
            title.Text = status + " Items";
            title.Font = new Font("Segoe UI", 18, FontStyle.Bold);
            title.ForeColor = TextDark;
            title.Location = new Point(30, 25);
            title.AutoSize = true;
            tableCard.Controls.Add(title);

            Label sub = new Label();
            sub.Text = "Browse all " + status.ToLower() + " campus reports";
            sub.Font = new Font("Segoe UI", 9);
            sub.ForeColor = TextMuted;
            sub.Location = new Point(32, 55);
            sub.AutoSize = true;
            tableCard.Controls.Add(sub);

            DataGridView filteredGrid = new DataGridView();

            filteredGrid.Location = new Point(30, 95);
            filteredGrid.Size = new Size(
                tableCard.Width - 60,
                tableCard.Height - 125
            );

            filteredGrid.Anchor =
                AnchorStyles.Top |
                AnchorStyles.Bottom |
                AnchorStyles.Left |
                AnchorStyles.Right;

            StyleGrid(filteredGrid);

            filteredGrid.AutoGenerateColumns = false;
            filteredGrid.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.None;

            filteredGrid.Columns.Add("Id", "Id");
            filteredGrid.Columns["Id"].Visible = false;

            filteredGrid.Columns.Add("No", "No.");

            DataGridViewImageColumn imgCol = new DataGridViewImageColumn();
            imgCol.Name = "Image";
            imgCol.HeaderText = "Image";
            imgCol.ImageLayout = DataGridViewImageCellLayout.Zoom;
            filteredGrid.Columns.Add(imgCol);

            filteredGrid.Columns.Add("ItemName", "Item");
            filteredGrid.Columns.Add("DateReported", "Date Reported");
            filteredGrid.Columns.Add("Category", "Category");
            filteredGrid.Columns.Add("Location", "Location");
            filteredGrid.Columns.Add("Description", "Description");
            filteredGrid.Columns.Add("Reporter", "Reporter");
            if (status == "Lost")
            {
                filteredGrid.Columns.Add("Claim", "Claim");
            }

            filteredGrid.Columns["No"].Width = 60;
            filteredGrid.Columns["Image"].Width = 90;
            filteredGrid.Columns["ItemName"].Width = 180;
            filteredGrid.Columns["DateReported"].Width = 170;
            filteredGrid.Columns["Category"].Width = 150;
            filteredGrid.Columns["Location"].Width = 170;
            filteredGrid.Columns["Description"].Width = 280;
            filteredGrid.Columns["Reporter"].Width = 170;
            if (status == "Lost")
            {
                filteredGrid.Columns["Claim"].Width = 120;

                filteredGrid.Columns["Claim"].DefaultCellStyle.BackColor = BurgundyLight;
                filteredGrid.Columns["Claim"].DefaultCellStyle.ForeColor = Burgundy;
                filteredGrid.Columns["Claim"].DefaultCellStyle.Alignment =
                    DataGridViewContentAlignment.MiddleCenter;
            }

            var items = DatabaseHelper.GetItems("")
                .Where(i => i.Status == status)
                .ToList();

            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];

                Image img = null;

                if (!string.IsNullOrEmpty(item.ImagePath) && File.Exists(item.ImagePath))
                {
                    using (var temp = new Bitmap(item.ImagePath))
                    {
                        img = new Bitmap(temp);
                    }
                }

                filteredGrid.Rows.Add(
                    item.Id,
                    i + 1,
                    img,
                    item.ItemName,
                    item.DateReported.ToString("MMM dd, yyyy hh:mm tt"),
                    item.Category,
                    item.Location,
                    item.Description,
                    item.Reporter
                );

                if (status == "Lost")
                {
                    filteredGrid.Rows[filteredGrid.Rows.Count - 1].Cells["Claim"].Value = "Claim";
                }


            }

            tableCard.Controls.Add(filteredGrid);

            Label emptyLabel = new Label();
            emptyLabel.Text = "No " + status.ToLower() + " items found.\nReports will appear here once available.";
            emptyLabel.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            emptyLabel.ForeColor = TextMuted;
            emptyLabel.TextAlign = ContentAlignment.MiddleCenter;
            emptyLabel.Location = new Point(0, 180);
            emptyLabel.Size = new Size(tableCard.Width, 90);
            emptyLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            emptyLabel.Visible = items.Count == 0;

            tableCard.Controls.Add(emptyLabel);
            emptyLabel.BringToFront();

            filteredGrid.Visible = items.Count > 0;

            filteredGrid.CellClick += (s, e) =>
            {
                if (e.RowIndex < 0)
                    return;

                if (filteredGrid.Columns[e.ColumnIndex].Name == "Image")
                {
                    var selectedItem = items[e.RowIndex];

                    if (string.IsNullOrEmpty(selectedItem.ImagePath) ||
                        !File.Exists(selectedItem.ImagePath))
                    {
                        ShowError("No image available.");
                        return;
                    }

                    ShowImagePreview(selectedItem);
                    return;
                }

                if (filteredGrid.Columns[e.ColumnIndex].Name == "Claim")
                {
                    var selectedItem = items[e.RowIndex];

                    Form claimForm = new Form();

                    claimForm.Text = "Claim Item";
                    claimForm.Size = new Size(420, 320);
                    claimForm.StartPosition =
                        FormStartPosition.CenterParent;

                    claimForm.BackColor = PageBg;

                    Label lblName = new Label();
                    lblName.Text = "Your Name";
                    lblName.Location = new Point(30, 30);
                    lblName.AutoSize = true;

                    TextBox txtName = CreateStyledTextBox(
                        30,
                        55,
                        330
                    );

                    Label lblProof = new Label();
                    lblProof.Text = "Proof / Details";
                    lblProof.Location = new Point(30, 100);
                    lblProof.AutoSize = true;

                    TextBox txtProof = new TextBox();

                    txtProof.Location = new Point(30, 125);
                    txtProof.Size = new Size(330, 80);
                    txtProof.Multiline = true;
                    txtProof.Font = new Font("Segoe UI", 10);

                    Button btnSubmit = CreatePrimaryButton(
                        "Submit Request",
                        30,
                        225,
                        150,
                        40
                    );

                    btnSubmit.Click += (a, b) =>
                    {
                        ClaimRequest request = new ClaimRequest()
                        {
                            ItemId = selectedItem.Id,
                            ItemName = selectedItem.ItemName,
                            Category = selectedItem.Category,
                            ClaimerName = txtName.Text,
                            ProofDetails = txtProof.Text,
                            RequestStatus = "Pending"
                        };

                        DatabaseHelper.AddClaimRequest(request);

                        ShowSuccess("Claim request submitted successfully.");

                        claimForm.Close();
                    };

                    claimForm.Controls.Add(lblName);
                    claimForm.Controls.Add(txtName);
                    claimForm.Controls.Add(lblProof);
                    claimForm.Controls.Add(txtProof);
                    claimForm.Controls.Add(btnSubmit);

                    claimForm.ShowDialog();
                }
            };
        }

        void ShowClaimRequestsPage()
        {
            dashboardTitle.Text = "Claim Requests";
            ClearContent();

            Label title = new Label();
            title.Text = "Claim Requests";
            title.Font = new Font("Segoe UI", 18, FontStyle.Bold);
            title.ForeColor = TextDark;
            title.Location = new Point(40, 30);
            title.Size = new Size(300, 40);
            contentPanel.Controls.Add(title);

            Panel card = CreateCard(10, 80, contentPanel.Width - 20, contentPanel.Height - 100);
            card.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            contentPanel.Controls.Add(card);
            SlideIn(card);

            DataGridView claimsGrid = new DataGridView();
            StyleGrid(claimsGrid);
            claimsGrid.Location = new Point(30, 85);
            claimsGrid.Size = new Size(card.Width - 60, card.Height - 115);
            Label cardTitle = new Label();
            cardTitle.Text = "Claim Management";
            cardTitle.Font = new Font("Segoe UI", 15, FontStyle.Bold);
            cardTitle.ForeColor = TextDark;
            cardTitle.Location = new Point(30, 25);
            cardTitle.AutoSize = true;
            card.Controls.Add(cardTitle);

            Label cardSub = new Label();
            cardSub.Text = "Review, approve, or reject submitted claim requests.";
            cardSub.Font = new Font("Segoe UI", 9);
            cardSub.ForeColor = TextMuted;
            cardSub.Location = new Point(32, 55);
            cardSub.AutoSize = true;
            card.Controls.Add(cardSub);
            claimsGrid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            claimsGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            claimsGrid.Columns.Add("Id", "Id");
            claimsGrid.Columns["Id"].Visible = false;
            claimsGrid.Columns.Add("No", "No.");
            claimsGrid.Columns.Add("ItemName", "Item Name");
            claimsGrid.Columns.Add("Category", "Category");
            claimsGrid.Columns.Add("ClaimerName", "Claimer");
            claimsGrid.Columns.Add("ProofDetails", "Proof Details");
            claimsGrid.Columns.Add("RequestStatus", "Status");

            if (loggedInUser != null && loggedInUser.Role == "Admin")
            {
                claimsGrid.Columns.Add("Approve", "Approve");
                claimsGrid.Columns.Add("Reject", "Reject");
            }

            var claims = DatabaseHelper.GetClaimRequests();

            for (int i = 0; i < claims.Count; i++)
            {
                var claim = claims[i];

                if (loggedInUser != null && loggedInUser.Role == "Admin")
                {
                    claimsGrid.Rows.Add(
                        claim.Id,
                        i + 1,
                        claim.ItemName,
                        claim.Category,
                        claim.ClaimerName,
                        claim.ProofDetails,
                        claim.RequestStatus,
                        "Approve",
                        "Reject"
                    );
                }
                else
                {
                    claimsGrid.Rows.Add(
                        claim.Id,
                        i + 1,
                        claim.ItemName,
                        claim.Category,
                        claim.ClaimerName,
                        claim.ProofDetails,
                        claim.RequestStatus
                    );
                }
            }

            claimsGrid.CellClick += (s, e) =>
            {
                if (e.RowIndex < 0) return;
                if (loggedInUser == null || loggedInUser.Role != "Admin") return;

                string columnName = claimsGrid.Columns[e.ColumnIndex].Name;
                int claimId = Convert.ToInt32(claimsGrid.Rows[e.RowIndex].Cells["Id"].Value);

                ClaimRequest claim = DatabaseHelper.GetClaimRequests()
                    .FirstOrDefault(c => c.Id == claimId);

                if (claim == null) return;

                if (columnName == "Approve")
                {
                    claim.RequestStatus = "Approved";
                    DatabaseHelper.UpdateClaimRequest(claim);
                    RefreshDashboardStats();

                    var item = DatabaseHelper.GetItems("")
                        .FirstOrDefault(i => i.Id == claim.ItemId);

                    if (item != null)
                    {
                        item.ClaimStatus = "Claimed";
                        DatabaseHelper.UpdateItem(item);
                    }

                    MessageBox.Show("Claim request approved.");
                    ShowClaimRequestsPage();
                }
                else if (columnName == "Reject")
                {
                    claim.RequestStatus = "Rejected";
                    DatabaseHelper.UpdateClaimRequest(claim);
                    RefreshDashboardStats();

                    MessageBox.Show("Claim request rejected.");
                    ShowClaimRequestsPage();
                }
            };

            card.Controls.Add(claimsGrid);

            if (claims.Count == 0)
            {
                claimsGrid.Visible = false;

                Label empty = new Label();
                empty.Text = "No claim requests yet.";
                empty.Font = new Font("Segoe UI", 12, FontStyle.Bold);
                empty.ForeColor = Color.Gray;
                empty.AutoSize = true;
                empty.Location = new Point(30, 75);
                card.Controls.Add(empty);
            }
        }

        void ShowActivityLogsPage()
        {
            dashboardTitle.Text = "Activity Logs";
            ClearContent();

            TextBox txtLogSearch = CreateStyledTextBox(20, 25, 360);
            txtLogSearch.Text = "Search logs...";
            txtLogSearch.ForeColor = Color.Gray;
            contentPanel.Controls.Add(txtLogSearch);

            Button btnExport = CreatePrimaryButton("Export Logs", 395, 25, 130, 36);
            contentPanel.Controls.Add(btnExport);

            Button btnClearLogs = new Button();
            btnClearLogs.Text = "Clear Logs";
            btnClearLogs.Location = new Point(540, 25);
            btnClearLogs.Size = new Size(130, 36);
            btnClearLogs.FlatStyle = FlatStyle.Flat;
            btnClearLogs.FlatAppearance.BorderSize = 0;
            btnClearLogs.BackColor = BurgundyLight;
            btnClearLogs.ForeColor = Burgundy;
            btnClearLogs.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            btnClearLogs.Cursor = Cursors.Hand;
            contentPanel.Controls.Add(btnClearLogs);

            btnClearLogs.Paint += (s, e) =>
            {
                btnClearLogs.Region = new Region(GetRoundedRect(btnClearLogs.ClientRectangle, 14));
            };

            Panel card = CreateCard(10, 85, contentPanel.Width - 20, contentPanel.Height - 120);
            card.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            contentPanel.Controls.Add(card);

            Label cardTitle = new Label();
            cardTitle.Text = "System Logs";
            cardTitle.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            cardTitle.ForeColor = TextDark;
            cardTitle.Location = new Point(30, 25);
            cardTitle.AutoSize = true;
            card.Controls.Add(cardTitle);

            Label cardSub = new Label();
            cardSub.Text = "Search, review, and export all recorded system activities.";
            cardSub.Font = new Font("Segoe UI", 9);
            cardSub.ForeColor = TextMuted;
            cardSub.Location = new Point(32, 57);
            cardSub.AutoSize = true;
            card.Controls.Add(cardSub);

            DataGridView logsGrid = new DataGridView();
            logsGrid.Location = new Point(30, 95);
            logsGrid.Size = new Size(card.Width - 60, card.Height - 125);
            logsGrid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            StyleGrid(logsGrid);
            logsGrid.AutoGenerateColumns = false;
            logsGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            logsGrid.RowTemplate.Height = 58;

            logsGrid.Columns.Add("Action", "Action");
            logsGrid.Columns.Add("PerformedBy", "Performed By");
            logsGrid.Columns.Add("Role", "Role");
            logsGrid.Columns.Add("DateCreated", "Date");

            logsGrid.Columns["Action"].FillWeight = 40;
            logsGrid.Columns["PerformedBy"].FillWeight = 22;
            logsGrid.Columns["Role"].FillWeight = 13;
            logsGrid.Columns["DateCreated"].FillWeight = 25;

            card.Controls.Add(logsGrid);

            void LoadLogs(string search = "")
            {
                logsGrid.Rows.Clear();

                var logs = DatabaseHelper.GetLogs();

                if (!string.IsNullOrWhiteSpace(search) && search != "Search logs...")
                {
                    search = search.ToLower();

                    logs = logs.Where(log =>
                        log.Action.ToLower().Contains(search) ||
                        log.PerformedBy.ToLower().Contains(search) ||
                        log.Role.ToLower().Contains(search) ||
                        log.DateCreated.ToString("MMM dd, yyyy hh:mm tt").ToLower().Contains(search)
                    ).ToList();
                }

                foreach (var log in logs)
                {
                    logsGrid.Rows.Add(
                        log.Action,
                        log.PerformedBy,
                        log.Role,
                        log.DateCreated.ToString("MMM dd, yyyy hh:mm tt")
                    );
                }

                logsGrid.ClearSelection();
                logsGrid.CurrentCell = null;
            }

            LoadLogs();

            txtLogSearch.Enter += (s, e) =>
            {
                if (txtLogSearch.Text == "Search logs...")
                {
                    txtLogSearch.Text = "";
                    txtLogSearch.ForeColor = TextDark;
                }
            };

            txtLogSearch.Leave += (s, e) =>
            {
                if (txtLogSearch.Text == "")
                {
                    txtLogSearch.Text = "Search logs...";
                    txtLogSearch.ForeColor = Color.Gray;
                    LoadLogs();
                }
            };

            txtLogSearch.TextChanged += (s, e) =>
            {
                LoadLogs(txtLogSearch.Text);
            };

            btnExport.Click += (s, e) =>
            {
                SaveFileDialog save = new SaveFileDialog();
                save.Filter = "Text File|*.txt";
                save.FileName = "ActivityLogs.txt";

                if (save.ShowDialog() == DialogResult.OK)
                {
                    var logs = DatabaseHelper.GetLogs();
                    List<string> lines = new List<string>();

                    lines.Add("CAMPUS LOST & FOUND SYSTEM");
                    lines.Add("ACTIVITY LOG REPORT");
                    lines.Add("");

                    foreach (var log in logs)
                    {
                        lines.Add(
                            "[" + log.DateCreated.ToString("MMM dd, yyyy hh:mm tt") + "] " +
                            log.PerformedBy + " (" + log.Role + ") - " +
                            log.Action
                        );
                    }

                    File.WriteAllLines(save.FileName, lines);
                    ShowSuccess("Logs exported successfully.");
                }
            };

            btnClearLogs.Click += (s, e) =>
            {
                if (ShowConfirm("Are you sure you want to delete this item?"))
                {
                    DatabaseHelper.ClearLogs();
                    MessageBox.Show("Activity logs cleared.");
                    ShowActivityLogsPage();
                    RefreshDashboardStats();
                }
            };
        }

        void ShowAdminPage()
        {
            dashboardTitle.Text = "Admin Panel";
            ClearContent();

            Label title = new Label();
            title.Text = "Admin Panel";
            title.Font = new Font("Segoe UI", 18, FontStyle.Bold);
            title.ForeColor = TextDark;
            title.Location = new Point(35, 30);
            title.Size = new Size(300, 40);
            contentPanel.Controls.Add(title);

            Button btnViewUsers = CreatePrimaryButton("View Registered Users", 35, 105, 230, 42);
            contentPanel.Controls.Add(btnViewUsers);

            btnViewUsers.Click += (s, e) => ShowRegisteredUsersPanel();

            Panel adminCard = CreateCard(10, 175, contentPanel.Width - 20, contentPanel.Height - 195);
            adminCard.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            contentPanel.Controls.Add(adminCard);
            SlideIn(adminCard);

            DataGridView adminGrid = new DataGridView();
            StyleGrid(adminGrid);
            adminGrid.Location = new Point(30, 85);
            adminGrid.Size = new Size(adminCard.Width - 60, adminCard.Height - 115);


            Label cardTitle = new Label();
            cardTitle.Text = "Lost Item Management";
            cardTitle.Font = new Font("Segoe UI", 15, FontStyle.Bold);
            cardTitle.ForeColor = TextDark;
            cardTitle.Location = new Point(30, 25);
            cardTitle.AutoSize = true;
            adminCard.Controls.Add(cardTitle);

            Label cardSub = new Label();
            cardSub.Text = "Remove invalid, duplicate, or outdated lost item reports.";
            cardSub.Font = new Font("Segoe UI", 9);
            cardSub.ForeColor = TextMuted;
            cardSub.Location = new Point(32, 55);
            cardSub.AutoSize = true;
            adminCard.Controls.Add(cardSub);

            adminGrid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            adminGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            adminGrid.Columns.Add("Id", "Id");
            adminGrid.Columns["Id"].Visible = false;
            adminGrid.Columns.Add("No", "No.");
            adminGrid.Columns.Add("Category", "Category");
            adminGrid.Columns.Add("Location", "Location");
            adminGrid.Columns.Add("Description", "Description");
            adminGrid.Columns.Add("Reporter", "Reporter");
            adminGrid.Columns.Add("Delete", "Delete");

            adminGrid.Columns["Delete"].DefaultCellStyle.BackColor = BurgundyLight;
            adminGrid.Columns["Delete"].DefaultCellStyle.ForeColor = Burgundy;
            adminGrid.Columns["Delete"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            adminGrid.Columns["Delete"].DefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);

            var lostItems = DatabaseHelper.GetItems("")
                .Where(i => i.Status == "Lost")
                .ToList();

            for (int i = 0; i < lostItems.Count; i++)
            {
                var item = lostItems[i];

                adminGrid.Rows.Add(
                    item.Id,
                    i + 1,
                    item.Category,
                    item.Location,
                    item.Description,
                    item.Reporter,
                    "🗑 Delete"
                );
            }
            adminGrid.ClearSelection();
            adminGrid.CurrentCell = null;

            adminGrid.CellClick += (s, e) =>
            {
                if (e.RowIndex < 0) return;

                if (adminGrid.Columns[e.ColumnIndex].Name == "Delete")
                {
                    int id = Convert.ToInt32(adminGrid.Rows[e.RowIndex].Cells["Id"].Value);

                    if (ShowConfirm("Are you sure you want to remove this lost item?"))
                    {
                        DatabaseHelper.DeleteItem(id);
                        RefreshDashboardStats();
                        MessageBox.Show("Lost item removed successfully.");
                        ShowAdminPage();
                    }
                }
            };

            adminCard.Controls.Add(adminGrid);
        }

        void ShowRegisteredUsersPanel()
        {
            Form usersForm = new Form();
            usersForm.Text = "Registered Users";
            usersForm.Size = new Size(750, 450);
            usersForm.StartPosition = FormStartPosition.CenterParent;
            usersForm.BackColor = PageBg;

            Panel card = CreateCard(20, 20, 700, 370);
            card.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            usersForm.Controls.Add(card);

            Label title = new Label();
            title.Text = "Registered Users";
            title.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            title.ForeColor = TextDark;
            title.Location = new Point(25, 20);
            title.Size = new Size(250, 35);
            card.Controls.Add(title);

            DataGridView usersGrid = new DataGridView();
            StyleGrid(usersGrid);
            usersGrid.Location = new Point(25, 70);
            usersGrid.Size = new Size(card.Width - 50, card.Height - 95);
            usersGrid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            usersGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            usersGrid.Columns.Add("No", "No.");
            usersGrid.Columns.Add("FullName", "Full Name");
            usersGrid.Columns.Add("Username", "Username");
            usersGrid.Columns.Add("Role", "Role");

            var users = DatabaseHelper.GetAllUsers();

            for (int i = 0; i < users.Count; i++)
            {
                usersGrid.Rows.Add(
                    i + 1,
                    users[i].FullName,
                    users[i].Username,
                    users[i].Role
                );
            }

            card.Controls.Add(usersGrid);
            usersForm.ShowDialog();
        }

        void ShowEditItemForm(LostFoundItem item)
        {
            Form editForm = new Form();
            editForm.Text = "Edit Item";
            editForm.Size = new Size(520, 620);
            editForm.StartPosition = FormStartPosition.CenterParent;
            editForm.BackColor = PageBg;
            editForm.FormBorderStyle = FormBorderStyle.FixedDialog;
            editForm.MaximizeBox = false;

            Panel card = CreateCard(20, 20, 460, 520);
            editForm.Controls.Add(card);

            Label title = new Label();
            title.Text = "Edit Item";
            title.Font = new Font("Segoe UI", 18, FontStyle.Bold);
            title.ForeColor = TextDark;
            title.Location = new Point(30, 25);
            title.Size = new Size(250, 35);
            card.Controls.Add(title);

            TextBox txtCategory = CreatePremiumInput(card, "Category", "Ex: Electronics", 30, 80, 390);
            txtCategory.Text = item.Category;

            TextBox txtStatus = CreatePremiumInput(card, "Status", "Ex: Lost", 30, 150, 390);
            txtStatus.Text = item.Status;

            TextBox txtLocation = CreatePremiumInput(card, "Location", "Ex: EA Room 301", 30, 220, 390);
            txtLocation.Text = item.Location;

            TextBox txtDescription = CreatePremiumInput(card, "Description", "Ex: Black iPhone 12", 30, 290, 390);
            txtDescription.Text = item.Description;

            TextBox txtReporter = CreatePremiumInput(card, "Reporter", "Please Enter your Full Username", 30, 360, 390);
            txtReporter.Text = item.Reporter;

            Button btnSave = CreatePrimaryButton("Save Changes", 30, 445, 390, 42);
            card.Controls.Add(btnSave);

            btnSave.Click += (s, e) =>
            {
                item.Category = txtCategory.Text;
                item.Status = txtStatus.Text;
                item.Location = txtLocation.Text;
                item.Description = txtDescription.Text;
                item.Reporter = txtReporter.Text;

                DatabaseHelper.UpdateItem(item);

                editForm.Close();
                ShowDashboardPage();

                ShowSuccess("Item updated successfully.");
            };

            editForm.ShowDialog();
        }
        int CountItems(string status)
        {
            return DatabaseHelper.GetItems("").Count(item => item.Status == status);
        }

        int CountClaims(string claimStatus)
        {
            return DatabaseHelper.GetItems("").Count(item => item.ClaimStatus == claimStatus);
        }

        int CountAllItems()
        {
            return DatabaseHelper.GetItems("").Count;
        }

        string GetMostCommonCategory()
        {
            var items = DatabaseHelper.GetItems("");

            if (items.Count == 0)
                return "N/A";

            return items
                .GroupBy(i => i.Category)
                .OrderByDescending(g => g.Count())
                .First()
                .Key;
        }

        string GetLatestReport()
        {
            var items = DatabaseHelper.GetItems("");

            if (items.Count == 0)
                return "No reports";

            return items
                .OrderByDescending(i => i.DateReported)
                .First()
                .ItemName;
        }

        string GetClaimRate()
        {
            var items = DatabaseHelper.GetItems("");

            if (items.Count == 0)
                return "0%";

            double claimed = items.Count(i => i.ClaimStatus == "Claimed");

            double rate = (claimed / items.Count) * 100;

            return rate.ToString("0") + "%";
        }

        

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            LoadData();
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            if (gridItems == null) return;
            LoadData();
        }

        private void GridItems_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            string columnName = gridItems.Columns[e.ColumnIndex].Name;

            if (columnName == "Image")
            {
                int id = Convert.ToInt32(gridItems.Rows[e.RowIndex].Cells["Id"].Value);

                LostFoundItem item = DatabaseHelper.GetItems("")
                    .FirstOrDefault(x => x.Id == id);

                if (item == null || string.IsNullOrEmpty(item.ImagePath) || !File.Exists(item.ImagePath))
                {
                    ShowError("No image available for this item.");
                    return;
                }

                ShowImagePreview(item);
                return;
            }

            if (columnName == "Delete")
            {
                int id = Convert.ToInt32(gridItems.Rows[e.RowIndex].Cells["Id"].Value);

                LostFoundItem item = DatabaseHelper.GetItems("")
                    .FirstOrDefault(x => x.Id == id);

                if (!CanManageItem(item))
                {
                    ShowError("You can only delete your own reports.");
                    return;
                }

                if (ShowConfirm("Are you sure you want to delete this item?"))
                {
                    DatabaseHelper.DeleteItem(id);

                    ShowDashboardPage();

                    ShowSuccess("Item deleted successfully.");
                }
            }
            else if (columnName == "Edit")
            {
                int id = Convert.ToInt32(gridItems.Rows[e.RowIndex].Cells["Id"].Value);

                LostFoundItem item = DatabaseHelper.GetItems("")
                    .FirstOrDefault(x => x.Id == id);

                if (item == null) return;
                if (!CanManageItem(item))
                {
                    ShowError("You can only edit your own reports.");
                    return;
                }

                ShowEditItemForm(item);
            }
        }

        private void RemovePlaceholder(object sender, EventArgs e)
        {
            if (txtSearch.Text == "Search item, category, or location...")
            {
                txtSearch.Text = "";
                txtSearch.ForeColor = Color.Black;
            }
        }

        private void AddPlaceholder(object sender, EventArgs e)
        {
            if (txtSearch.Text == "")
            {
                txtSearch.Text = "Search item, category, or location...";
                txtSearch.ForeColor = Color.Gray;
            }
        }

        void ShowSuccess(string message)
        {
            Form popup = new Form();

            popup.Size = new Size(440, 240);
            popup.StartPosition = FormStartPosition.CenterParent;
            popup.FormBorderStyle = FormBorderStyle.None;
            popup.BackColor = Color.White;
            popup.ShowInTaskbar = false;

            popup.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                Rectangle rect = new Rectangle(0, 0, popup.Width - 1, popup.Height - 1);

                using (GraphicsPath path = GetRoundedRect(rect, 20))
                using (Pen border = new Pen(Color.FromArgb(220, 225, 235), 1))
                {
                    popup.Region = new Region(path);
                    e.Graphics.DrawPath(border, path);
                }
            };

            

            Label title = new Label();
            title.Text = "Success";
            title.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            title.ForeColor = TextDark;
            title.Location = new Point(95, 88);
            title.Size = new Size(250, 40);
            title.TextAlign = ContentAlignment.MiddleCenter;
            popup.Controls.Add(title);

            Label msg = new Label();
            msg.Text = message;
            msg.Font = new Font("Segoe UI", 10);
            msg.ForeColor = TextMuted;
            msg.TextAlign = ContentAlignment.MiddleCenter;
            msg.Location = new Point(35, 128);
            msg.Size = new Size(370, 40);
            popup.Controls.Add(msg);

            Button btnOk = CreatePrimaryButton("OK", (popup.Width - 130) / 2, 180, 130, 40);

            btnOk.Click += (s, e) => popup.Close();

            popup.Controls.Add(btnOk);

            popup.ShowDialog();
        }

        void ShowImagePreview(LostFoundItem item)
        {
            Form popup = new Form();

            popup.FormBorderStyle = FormBorderStyle.None;
            popup.StartPosition = FormStartPosition.CenterParent;
            popup.BackColor = Color.White;
            popup.Size = new Size(760, 620);
            popup.ShowInTaskbar = false;

            popup.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                Rectangle rect = new Rectangle(0, 0, popup.Width - 1, popup.Height - 1);

                using (GraphicsPath path = GetRoundedRect(rect, 28))
                using (Pen border = new Pen(Color.FromArgb(225, 228, 235), 1))
                {
                    popup.Region = new Region(path);
                    e.Graphics.DrawPath(border, path);
                }
            };

            Label title = new Label();
            title.Text = item.ItemName;
            title.Font = new Font("Segoe UI", 20, FontStyle.Bold);
            title.ForeColor = TextDark;
            title.Location = new Point(35, 28);
            title.Size = new Size(500, 40);
            popup.Controls.Add(title);

            Label sub = new Label();
            sub.Text = item.Category + " • " + item.Status;
            sub.Font = new Font("Segoe UI", 10);
            sub.ForeColor = TextMuted;
            sub.Location = new Point(38, 70);
            sub.Size = new Size(300, 25);
            popup.Controls.Add(sub);

            Button closeBtn = new Button();
            closeBtn.Text = "✕";
            closeBtn.FlatStyle = FlatStyle.Flat;
            closeBtn.FlatAppearance.BorderSize = 0;
            closeBtn.BackColor = Color.Transparent;
            closeBtn.ForeColor = TextMuted;
            closeBtn.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            closeBtn.Size = new Size(40, 40);
            closeBtn.Location = new Point(680, 20);
            closeBtn.Cursor = Cursors.Hand;

            closeBtn.MouseEnter += (s, e) =>
            {
                closeBtn.ForeColor = Burgundy;
            };

            closeBtn.MouseLeave += (s, e) =>
            {
                closeBtn.ForeColor = TextMuted;
            };

            closeBtn.Click += (s, e) =>
            {
                popup.Close();
            };

            popup.Controls.Add(closeBtn);

            Panel imageCard = new Panel();
            imageCard.Location = new Point(35, 110);
            imageCard.Size = new Size(690, 430);
            imageCard.BackColor = Color.FromArgb(248, 249, 252);

            imageCard.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                Rectangle rect = new Rectangle(0, 0, imageCard.Width - 1, imageCard.Height - 1);

                using (GraphicsPath path = GetRoundedRect(rect, 22))
                using (Pen border = new Pen(Color.FromArgb(230, 232, 238), 1))
                {
                    imageCard.Region = new Region(path);
                    e.Graphics.DrawPath(border, path);
                }
            };

            popup.Controls.Add(imageCard);

            PictureBox picture = new PictureBox();
            picture.Dock = DockStyle.Fill;
            picture.SizeMode = PictureBoxSizeMode.Zoom;
            picture.BackColor = Color.Transparent;

            using (var temp = new Bitmap(item.ImagePath))
            {
                picture.Image = new Bitmap(temp);
            }

            imageCard.Controls.Add(picture);

            Button btnClose = CreatePrimaryButton(
                "Close",
                (popup.Width - 160) / 2,
                555,
                160,
                42
            );

            btnClose.Click += (s, e) => popup.Close();

            popup.Controls.Add(btnClose);

            popup.ShowDialog();
        }

        void SlideIn(Control control)
        {
            int finalX = control.Left;
            int startX = finalX + 40;

            control.Left = startX;

            Timer timer = new Timer();
            timer.Interval = 10;

            timer.Tick += (s, e) =>
            {
                if (control.Left <= finalX)
                {
                    control.Left = finalX;
                    timer.Stop();
                    timer.Dispose();
                }
                else
                {
                    control.Left -= 4;
                }
            };

            timer.Start();
        }

        void ShowError(string message)
        {
            Form popup = new Form();
            popup.FormBorderStyle = FormBorderStyle.None;
            popup.StartPosition = FormStartPosition.CenterParent;
            popup.BackColor = Color.White;
            popup.Size = new Size(380, 220);

            popup.ShowInTaskbar = false;

            popup.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                using (Pen border = new Pen(Color.FromArgb(230, 230, 235), 1))
                {
                    e.Graphics.DrawRectangle(border, 0, 0,
                        popup.Width - 1,
                        popup.Height - 1);
                }
            };

           

            Label title = new Label();
            title.Text = "Error";
            title.Font = new Font("Segoe UI", 18, FontStyle.Bold);
            title.ForeColor = TextDark;
            title.Size = new Size(380, 40);
            title.TextAlign = ContentAlignment.MiddleCenter;
            title.Location = new Point(0, 45);
            popup.Controls.Add(title);

            Label desc = new Label();
            desc.Text = message;
            desc.Font = new Font("Segoe UI", 10);
            desc.ForeColor = TextMuted;
            desc.AutoSize = false;
            desc.Size = new Size(300, 40);
            desc.TextAlign = ContentAlignment.MiddleCenter;
            desc.Location = new Point(40, 95);
            popup.Controls.Add(desc);

            Button ok = new Button();
            ok.Text = "OK";
            ok.Size = new Size(110, 38);
            ok.Location = new Point(135, 150);

            ok.FlatStyle = FlatStyle.Flat;
            ok.FlatAppearance.BorderSize = 0;

            ok.BackColor = Burgundy;
            ok.ForeColor = Color.White;

            ok.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            ok.Cursor = Cursors.Hand;

            ok.Click += (s, e) => popup.Close();

            ok.Paint += (s, e) =>
            {
                ok.Region = new Region(GetRoundedRect(ok.ClientRectangle, 14));
            };

            popup.Controls.Add(ok);

            popup.Region = new Region(GetRoundedRect(popup.ClientRectangle, 24));

            popup.ShowDialog();
        }

        bool ShowConfirm(string message)
        {
            bool result = false;

            Form popup = new Form();
            popup.FormBorderStyle = FormBorderStyle.None;
            popup.StartPosition = FormStartPosition.CenterParent;
            popup.BackColor = Color.White;
            popup.Size = new Size(420, 240);
            popup.ShowInTaskbar = false;

            popup.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                using (Pen border = new Pen(Color.FromArgb(230, 230, 235), 1))
                {
                    e.Graphics.DrawRectangle(border, 0, 0,
                        popup.Width - 1,
                        popup.Height - 1);
                }
            };

            popup.Region = new Region(GetRoundedRect(popup.ClientRectangle, 24));

            Label title = new Label();
            title.Text = "Confirm Delete";
            title.Font = new Font("Segoe UI", 18, FontStyle.Bold);
            title.ForeColor = TextDark;
            title.Size = new Size(popup.Width, 40);
            title.Location = new Point(0, 35);
            title.TextAlign = ContentAlignment.MiddleCenter;
            popup.Controls.Add(title);

            Label desc = new Label();
            desc.Text = message;
            desc.Font = new Font("Segoe UI", 10);
            desc.ForeColor = TextMuted;
            desc.Size = new Size(320, 50);
            desc.Location = new Point(50, 85);
            desc.TextAlign = ContentAlignment.MiddleCenter;
            popup.Controls.Add(desc);

            Button btnYes = CreatePrimaryButton("Delete", 60, 165, 130, 42);
            popup.Controls.Add(btnYes);

            Button btnNo = new Button();
            btnNo.Text = "Cancel";
            btnNo.Location = new Point(225, 165);
            btnNo.Size = new Size(130, 42);
            btnNo.FlatStyle = FlatStyle.Flat;
            btnNo.FlatAppearance.BorderSize = 0;
            btnNo.BackColor = BurgundyLight;
            btnNo.ForeColor = Burgundy;
            btnNo.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            btnNo.Cursor = Cursors.Hand;

            btnNo.Paint += (s, e) =>
            {
                btnNo.Region = new Region(GetRoundedRect(btnNo.ClientRectangle, 14));
            };

            popup.Controls.Add(btnNo);

            btnYes.Click += (s, e) =>
            {
                result = true;
                popup.Close();
            };

            btnNo.Click += (s, e) =>
            {
                popup.Close();
            };

            popup.ShowDialog();

            return result;
        }



        void ClearContent()
        {
            contentPanel.Controls.Clear();
        }

        void RefreshDashboardStats()
        {
            LoadData();
        }

        bool CanManageItem(LostFoundItem item)
        {
            if (loggedInUser == null || item == null)
                return false;

            if (loggedInUser.Role == "Admin")
                return true;

            string reporter = (item.Reporter ?? "").Trim().ToLower();
            string fullName = (loggedInUser.FullName ?? "").Trim().ToLower();
            string username = (loggedInUser.Username ?? "").Trim().ToLower();

            return reporter == fullName || reporter == username;
        }

        GraphicsPath GetRoundedRect(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int d = radius * 2;

            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();

            return path;
        }
    }
}


