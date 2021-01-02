using SteamLogin.Classes;
using System.Drawing;
using System.Windows.Forms;

namespace SteamLogin
{
    public partial class ScreenManager : Form
    {

        private readonly Monitor SCREEN_LEFT = new Monitor("ScreenL", 1);
        private readonly Monitor SCREEN_MIDDLE = new Monitor("ScreenM", 1);
        private readonly Monitor SCREEN_RIGHT = new Monitor("ScreenR", 1);

        private readonly Color activeColour = Color.Green;
        private readonly Color inactiveColour = Color.Red;

        public ScreenManager()
        {
            InitializeComponent();
            SetStates();
        }

        // handle visual update of monitor state
        private void HandleState(Panel panel, Monitor monitor)
        {
            panel.BackColor = (monitor.State == 1) ? activeColour : inactiveColour;
            monitor.SetState((monitor.State == 1) ? 1 : 0);
            this.Text = $"Monitor: {monitor.Name} - State: " + monitor.State.ToString();
        }

        private void SetStates()
        {
            screenL.BackColor = (SCREEN_LEFT.State == 1) ? activeColour : inactiveColour;
            screenM.BackColor = (SCREEN_MIDDLE.State == 1) ? activeColour : inactiveColour;
            screenR.BackColor = (SCREEN_RIGHT.State == 1) ? activeColour : inactiveColour;
        }

        #region panel click events
        private void ScreenL_Click(object sender, System.EventArgs e)
        {
            int state = (SCREEN_LEFT.State == 1) ? 0 : 1;
            SCREEN_LEFT.SetState(state);

            HandleState(screenL, SCREEN_LEFT);
        }

        private void ScreenM_Click(object sender, System.EventArgs e)
        {
            int state = (SCREEN_MIDDLE.State == 1) ? 0 : 1;
            SCREEN_MIDDLE.SetState(state);

            HandleState(screenM, SCREEN_MIDDLE);
        }

        private void ScreenR_Click(object sender, System.EventArgs e)
        {
            int state = (SCREEN_RIGHT.State == 1) ? 0 : 1;
            SCREEN_RIGHT.SetState(state);

            HandleState(screenR, SCREEN_RIGHT);
        }
        #endregion
        
    }
}
