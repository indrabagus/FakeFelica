using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using com.esp.common;
using System.Threading;
using acr122u;
using nfc.felica;
using nfc.ndef;

namespace com.esp.fakefelica
{
    public partial class CardForm : Form
    {
        Acr122u rw;
        FakeFelica fFelica;

        public CardForm()
        {
            InitializeComponent();
            rw = new Acr122u();
        }
        private void btEmulate_Click(object sender, EventArgs e)
        {
            byte[] idm = Utility.HexToByte(tbIdm.Text);
            byte[] uri = Encoding.UTF8.GetBytes(tbUri.Text);

            ShortRecord record = new ShortRecord();
            record.Header.Tnf = TNF.NfcWkt;
            record.RecordType = Encoding.ASCII.GetBytes("U");//Well Known type -- URI
            record.Payload = new byte[1 + uri.Length];
            record.Payload[0] = 0x00;
            uri.CopyTo(record.Payload, 1);

            NdefMessage ndef = new NdefMessage();
            ndef.Record.Add(record);

            fFelica = new FakeFelica(rw);
            Type3TagController control = new Type3TagController(fFelica, idm, ndef);
            control.Init();
            fFelica.Start();
        }

        private void CardBankForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (fFelica != null)
                fFelica.Abort();
        }
    }
}
