using System;
using System.Windows.Forms;

namespace SpendPoint
{
    public partial class AlertForm : Form
    {
        private const int CP_NOCLOSE_BUTTON = 0x200;
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams myCp = base.CreateParams;
                myCp.ClassStyle = myCp.ClassStyle | CP_NOCLOSE_BUTTON;
                return myCp;
            }
        }

        #region PROPERTIES

        public string Message
        {
            set { labelMessage.Text = value; }
        }

        public int ProgressValue
        {
            set
            {
                if (value >= 0 && value <= 100)
                    progressBar1.Value = value;
                lblProgress.Text = value + "%";
            }
        }

        public int RecordValue
        {
            set
            {
                //progressBar1.Value = value;
                lblRecords.Text = "Records Processed : " + value;
            }
        }

        public string FileValue
        {
            set
            {
                //progressBar1.Value = value;
                lblCurrentStage.Text = string.Format("{0}", value);
            }
        }

        public string image
        {
            set { pictureBox1.ImageLocation = value; }
        }

        #endregion

        #region METHODS

        public AlertForm()
        {
            InitializeComponent();
        }

        #endregion

        #region EVENTS

        public event EventHandler<EventArgs> Canceled;

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            // Create a copy of the event to work with
            EventHandler<EventArgs> ea = Canceled;
            /* If there are no subscribers, eh will be null so we need to check
             * to avoid a NullReferenceException. */
            if (ea != null)
                ea(this, e);
        }

        #endregion

        private void AlertForm_Load(object sender, EventArgs e)
        {

        }
    }
}
