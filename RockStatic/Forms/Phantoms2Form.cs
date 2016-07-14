﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RockStatic
{
    public partial class Phantoms2Form : Form
    {
        #region variables de clase

        /// <summary>
        /// Referencia al MainForm padre
        /// </summary>
        public MainForm padre;

        /// <summary>
        /// Referencia al form NewProjectForm que invoca
        /// </summary>
        public NewProjectForm newProjectForm;

        Point lastClick;

        #endregion

        public Phantoms2Form()
        {
            InitializeComponent();
        }

        private void lblTitulo_DoubleClick(object sender, EventArgs e)
        {
            CentrarForm();
        }

        /// <summary>
        /// Se centra el Form con respecto al MDI parent
        /// </summary>
        public void CentrarForm()
        {
            //this.Location = new System.Drawing.Point((Screen.PrimaryScreen.WorkingArea.Width - this.Width) / 2, (Screen.PrimaryScreen.WorkingArea.Height - this.Height) / 2);
            this.Location = new System.Drawing.Point((MdiParent.Width - this.Width) / 2, (int)((MdiParent.Height - this.Height) * 0.8 / 2));
        }

        private void btnSubir_MouseEnter(object sender, EventArgs e)
        {
            ((System.Windows.Forms.Button)(sender)).ForeColor = Color.White;
        }

        private void btnSubir_MouseLeave(object sender, EventArgs e)
        {
            ((System.Windows.Forms.Button)(sender)).ForeColor = Color.Black;
        }

        private void Phantoms2Form_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.padre.CerrarPhantom2Form();
        }

        private void lblTitulo_MouseDown(object sender, MouseEventArgs e)
        {
            lastClick = e.Location;
        }

        private void lblTitulo_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.Left += e.X - lastClick.X;
                this.Top += e.Y - lastClick.Y;
            }
        }

        private void Phantoms2Form_Load(object sender, EventArgs e)
        {
            // se carga la información a mostrar en pantalla desde NewProjectForm

            numDensP1.Value = (decimal)newProjectForm.tempPhantom1High.densidad;
            numDensP2.Value = (decimal)newProjectForm.tempPhantom2High.densidad;
            numDensP3.Value = (decimal)newProjectForm.tempPhantom3High.densidad;

            numZeffP1.Value = (decimal)newProjectForm.tempPhantom1High.zeff;
            numZeffP2.Value = (decimal)newProjectForm.tempPhantom2High.zeff;
            numZeffP3.Value = (decimal)newProjectForm.tempPhantom3High.zeff;
        }

        public void btnCerrar_Click(object sender, EventArgs e)
        {
            // se guarda la informacion modificada y se cierra el form

            newProjectForm.tempPhantom1High.densidad = (double)numDensP1.Value;
            newProjectForm.tempPhantom2High.densidad = (double)numDensP2.Value;
            newProjectForm.tempPhantom3High.densidad = (double)numDensP3.Value;

            newProjectForm.tempPhantom1High.zeff = (double)numZeffP1.Value;
            newProjectForm.tempPhantom2High.zeff = (double)numZeffP2.Value;
            newProjectForm.tempPhantom3High.zeff = (double)numZeffP3.Value;

            this.Close();
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Phantoms2Form_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawRectangle(new Pen(Color.Green, 2), this.DisplayRectangle);       
        }
    }
}