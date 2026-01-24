using Serilog;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace SkoprRecord.WinForms.Controls;

public partial class CustomTitleBar : UserControl
{
    private Form? _parentForm;
    private PictureBox _picIcon = null!;
    private Label _lblTitle = null!;
    private Button _btnMinimize = null!;
    private Button _btnClose = null!;
    private Button _btnSettings = null!;

    [Category("Appearance")]
    public string Title
    {
        get => _lblTitle.Text;
        set => _lblTitle.Text = value;
    }

    [Category("Behavior")]
    public bool ShowMinimize
    {
        get => _btnMinimize.Visible;
        set => _btnMinimize.Visible = value;
    }

    [Category("Behavior")]
    public bool ShowClose
    {
        get => _btnClose.Visible;
        set => _btnClose.Visible = value;
    }

    [Category("Behavior")]
    public bool ShowSettings
    {
        get => _btnSettings.Visible;
        set => _btnSettings.Visible = value;
    }

    public event EventHandler? SettingsClick;
    public event EventHandler? CloseRequested;

    public CustomTitleBar()
    {
        InitializeComponent();
        this.Dock = DockStyle.Top;
        this.BackColor = Color.FromArgb(25, 25, 40);
        this.Height = 40;
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        if (!DesignMode)
        {
            _parentForm = this.FindForm();
            LoadIcon();
        }
    }

    private void InitializeComponent()
    {
        _picIcon = new PictureBox();
        _lblTitle = new Label();
        _btnMinimize = new Button();
        _btnClose = new Button();
        _btnSettings = new Button();

        ((ISupportInitialize)_picIcon).BeginInit();
        SuspendLayout();

        // 
        // picIcon
        // 
        _picIcon.Location = new Point(10, 10);
        _picIcon.Size = new Size(20, 20);
        _picIcon.SizeMode = PictureBoxSizeMode.StretchImage;
        _picIcon.TabStop = false;

        // 
        // lblTitle
        // 
        _lblTitle.AutoSize = false;
        _lblTitle.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
        _lblTitle.ForeColor = Color.LightGray;
        _lblTitle.Location = new Point(35, 0);
        _lblTitle.Size = new Size(305, 40);
        _lblTitle.Text = "Application";
        _lblTitle.TextAlign = ContentAlignment.MiddleLeft;

        // 
        // btnClose
        // 
        _btnClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        _btnClose.BackColor = Color.Transparent;
        _btnClose.FlatAppearance.BorderSize = 0;
        _btnClose.FlatAppearance.MouseOverBackColor = Color.FromArgb(232, 17, 35);
        _btnClose.FlatStyle = FlatStyle.Flat;
        _btnClose.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
        _btnClose.ForeColor = Color.White;
        _btnClose.Location = new Point(this.Width - 40, 0);
        _btnClose.Size = new Size(40, 40);
        _btnClose.Text = "✕";
        _btnClose.UseVisualStyleBackColor = false;
        _btnClose.Click += (s, e) => CloseRequested?.Invoke(this, EventArgs.Empty);

        // 
        // btnMinimize
        // 
        _btnMinimize.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        _btnMinimize.BackColor = Color.Transparent;
        _btnMinimize.FlatAppearance.BorderSize = 0;
        _btnMinimize.FlatAppearance.MouseOverBackColor = Color.FromArgb(50, 50, 70);
        _btnMinimize.FlatStyle = FlatStyle.Flat;
        _btnMinimize.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
        _btnMinimize.ForeColor = Color.White;
        _btnMinimize.Location = new Point(this.Width - 80, 0);
        _btnMinimize.Size = new Size(40, 40);
        _btnMinimize.Text = "─";
        _btnMinimize.UseVisualStyleBackColor = false;
        _btnMinimize.Click += (s, e) => { if (_parentForm != null) _parentForm.WindowState = FormWindowState.Minimized; };

        // 
        // btnSettings
        // 
        _btnSettings.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        _btnSettings.BackColor = Color.Transparent;
        _btnSettings.FlatAppearance.BorderSize = 0;
        _btnSettings.FlatAppearance.MouseOverBackColor = Color.FromArgb(50, 50, 70);
        _btnSettings.FlatStyle = FlatStyle.Flat;
        _btnSettings.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point);
        _btnSettings.ForeColor = Color.White;
        _btnSettings.Location = new Point(this.Width - 120, 0);
        _btnSettings.Size = new Size(40, 40);
        _btnSettings.Text = "⚙";
        _btnSettings.UseVisualStyleBackColor = false;
        _btnSettings.Visible = false;
        _btnSettings.Click += (s, e) => SettingsClick?.Invoke(this, EventArgs.Empty);


        this.Controls.Add(_btnSettings);
        this.Controls.Add(_btnMinimize);
        this.Controls.Add(_btnClose);
        this.Controls.Add(_lblTitle);
        this.Controls.Add(_picIcon);

        // Sürükleme olayları
        this.MouseDown += CustomTitleBar_MouseDown;
        _lblTitle.MouseDown += CustomTitleBar_MouseDown;
        _picIcon.MouseDown += CustomTitleBar_MouseDown;

        ((ISupportInitialize)_picIcon).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);

        if (_btnClose == null || _btnMinimize == null || _btnSettings == null)
            return;

        // Pozisyonları sağdan sola doğru hesapla
        int rightPos = this.Width;

        // Kapat butonu (En sağda)
        if (_btnClose.Visible)
        {
            _btnClose.Location = new Point(rightPos - 40, 0);
            rightPos -= 40;
        }

        // Küçültme butonu
        if (_btnMinimize.Visible)
        {
            _btnMinimize.Location = new Point(rightPos - 40, 0);
            rightPos -= 40;
        }

        // Ayarlar butonu
        if (_btnSettings.Visible)
        {
            _btnSettings.Location = new Point(rightPos - 40, 0);
        }
    }

    private void LoadIcon()
    {
        try
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            // Resource adı: Varsayılan Namespace + Klasör Yolu + Dosya Adı
            // Örn: SkoprRecord.WinForms.Assets.app_icon.ico
            var resourceName = "SkoprRecord.WinForms.Assets.app_icon.ico";

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream != null)
                {
                    _picIcon.Image = new Icon(stream).ToBitmap();
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "TitleBar Icon yüklenirken hata.");
        }
    }

    // Sürükleme Mantığı
    [DllImport("user32.dll")]
    public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
    [DllImport("user32.dll")]
    public static extern bool ReleaseCapture();

    private void CustomTitleBar_MouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left && _parentForm != null)
        {
            ReleaseCapture();
            SendMessage(_parentForm.Handle, 0xA1, 0x2, 0);
        }
    }
}
