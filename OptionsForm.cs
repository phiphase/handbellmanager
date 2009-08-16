﻿// This file is part of Handbell Manager.
// Copyright Graham John 2009. graham@changeringing.co.uk
//
// Handbell Manager is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// Handbell Manager is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with Foobar.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace HandbellManager
{
	public partial class OptionsForm : Form
	{
		private MotionControllerManager.MotionControllerManager _mcm;
		private Handbell[] _hb;
		bool _initialisation;

		public OptionsForm(Handbell[] handbells, MotionControllerManager.MotionControllerManager mcm)
		{
			InitializeComponent();
			_mcm = mcm;
			_hb = handbells;
			InitializeFields();
		}
		private void InitializeFields()
		{
			_initialisation = true;
			//Handbell Calibration
			for (int i = 0; i < 4; i++)
			{
				tabHandbellCalibration.Controls["txtZBSP" + i].Text = Settings.zBSP[i].ToString();
				tabHandbellCalibration.Controls["txtZHSP" + i].Text = Settings.zHSP[i].ToString();
				//Enable calibration buttons
				if (i < _mcm.Count)
					tabHandbellCalibration.Controls["btnCalibrate" + i].Enabled = true;
				else
					tabHandbellCalibration.Controls["btnCalibrate" + i].Enabled = false;
			}

			spnDebounceDelay.Value = Settings.debounceDelay;
			spnDelayHS.Value = Settings.handstrokeStrikeDelay;
			spnDelayBS.Value = Settings.backstrokeStrikeDelay;

			//Abel keystrokes
			txtProcessName.Text = Settings.abelProcessName;
			for (int i = 0; i < 4; i++)
			{
				tabAbelKeyStrokes.Controls["txtKeyBS" + i].Text = Settings.keyBS[i];
				tabAbelKeyStrokes.Controls["txtKeyHS" + i].Text = Settings.keyHS[i];
				tabAbelKeyStrokes.Controls["txtKeyB1" + i].Text = Settings.keyB1[i];
				tabAbelKeyStrokes.Controls["txtKeyB2" + i].Text = Settings.keyB2[i];
			}
			chkUseKeyUpDown.Checked = Settings.useKeyUpDown;
			_initialisation = false;
		}

		private NumericalTextBox GetHandbellTextbox(string name, int index)
		{
			return (NumericalTextBox)tabHandbellCalibration.Controls[name + index];
		}

		private void txtProcessName_TextChanged(object sender, EventArgs e)
		{
			if (_initialisation)
				return;
			if (txtProcessName.Text.TrimEnd(' ').EndsWith(".exe", StringComparison.CurrentCultureIgnoreCase))
			{
				Point pnt = txtProcessName.GetPositionFromCharIndex(txtProcessName.TextLength - 1);
				pnt.X -= 8;
				pnt.Y -= 68;
				warningTip.ToolTipTitle = "Extension";
				warningTip.Show("Do not include .exe!", txtProcessName, pnt, int.MaxValue);

				System.Media.SystemSounds.Beep.Play();

			}
			else
			{
				warningTip.RemoveAll();
			}

			Settings.abelProcessName = txtProcessName.Text;
		}

		private void btnOK_Click(object sender, EventArgs e)
		{
			//Save settings and close
			Settings.Save();

			DialogResult = DialogResult.OK;
			Close();
		}

		private void btnCancel_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			//Restore settings and close
			Settings.Open();
			//Update handbell settings
			for (int i = 0; i < _mcm.Count; i++)
			{
				_hb[i].UpdateSettings();
			}
			Close();
		}

		public void Tick()
		{
			for (int i = 0; i < _mcm.Count; i++)
			{
				tabHandbellCalibration.Controls["txtZ" + i].Text = _hb[i].CurrentZ.ToString();
			}
		}

		private void btnCalibrate_Click(object sender, EventArgs e)
		{
			ConfigForm.sendToAbelEnabled = false;
			Button btn = (Button)sender;
			int i = Convert.ToInt32(btn.Name.Substring(btn.Name.Length - 1, 1));

			CalibrationForm cf = new CalibrationForm();
			Handbell newHB = cf.StartCalibration(_mcm, i);
			//Apply Calibration
			if (newHB != null)
			{
				_hb[i] = newHB;
				tabHandbellCalibration.Controls["txtZBSP" + i].Text = Settings.zBSP[i].ToString();
				tabHandbellCalibration.Controls["txtZHSP" + i].Text = Settings.zHSP[i].ToString();
			}
			ConfigForm.sendToAbelEnabled = true;
		}

		private void spnDebounceDelay_ValueChanged(object sender, EventArgs e)
		{
			if (_initialisation)
				return;
			Settings.debounceDelay = Convert.ToInt32(spnDebounceDelay.Value);
		}

		private void spnDelayBS_ValueChanged(object sender, EventArgs e)
		{
			if (_initialisation)
				return;
			Settings.backstrokeStrikeDelay = Convert.ToInt32(spnDelayBS.Value);
		}

		private void spnDelayHS_ValueChanged(object sender, EventArgs e)
		{
			if (_initialisation)
				return;
			Settings.handstrokeStrikeDelay = Convert.ToInt32(spnDelayHS.Value);
		}

		private void txtZBSP_TextChanged(object sender, EventArgs e)
		{
			if (_initialisation)
				return;
			
			for (int i = 0; i < 4; i++)
			{
				Settings.zBSP[i] = GetHandbellTextbox("txtZBSP", i).Value;
			}
			//Update handbell settings
			for (int i = 0; i < _mcm.Count; i++)
			{
				_hb[i].UpdateSettings();
			}

		}

		private void txtZHSP_TextChanged(object sender, EventArgs e)
		{
			if (_initialisation)
				return;

			for (int i = 0; i < 4; i++)
			{
				Settings.zHSP[i] = GetHandbellTextbox("txtZHSP", i).Value;
			}
			//Update handbell settings
			for (int i = 0; i < _mcm.Count; i++)
			{
				_hb[i].UpdateSettings();
			}

		}
		private void txtKey_TextChanged(object sender, EventArgs e)
		{
			if (_initialisation)
				return;
			for (int i = 0; i < 4; i++)
			{
				Settings.keyBS[i] = tabAbelKeyStrokes.Controls["txtKeyBS" + i].Text;
				Settings.keyHS[i] = tabAbelKeyStrokes.Controls["txtKeyHS" + i].Text;
				Settings.keyB1[i] = tabAbelKeyStrokes.Controls["txtKeyB1" + i].Text;
				Settings.keyB2[i] = tabAbelKeyStrokes.Controls["txtKeyB2" + i].Text;
			}
		}

		private void chkUseKeyUpDown_CheckedChanged(object sender, EventArgs e)
		{
			if (_initialisation)
				return;
			Settings.useKeyUpDown = chkUseKeyUpDown.Checked;
		}

		private void btnDefault_Click(object sender, EventArgs e)
		{
			//Confirm reset
			System.Windows.Forms.DialogResult response = 
				MessageBox.Show("Revert all Handbell Calibration and Abel Settings to their default values?", "Default Settings Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
			if (response == DialogResult.Yes)
			{
				//Set back to defaults
				Settings.Default();
				//Update handbell settings
				for (int i = 0; i < _mcm.Count; i++)
				{
					_hb[i].UpdateSettings();
				}
				InitializeFields();
			}
		}

		private void OptionsForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			Program.ConfigForm.OptionsClosed();
		}

	}
}