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
    public partial class SelectAreasForm : Form
    {
        #region variables de disenador

        /// <summary>
        /// Lapiz para dibujar el circulo seleccionado
        /// </summary>
        Pen pen2;

        /// <summary>
        /// Lista para guardar las areas seleccionadas
        /// </summary>
        List<CCuadrado> areasCore;

        /// <summary>
        /// Referencia al MainForm padre
        /// </summary>
        public MainForm padre;

        /// <summary>
        /// variable de control para evitar que se lance el evento Paint de pictCore
        /// </summary>
        bool controlPaint;

        /// <summary>
        /// Array que guarda las coordenadas de los clicks necesarios para dibujar un circulo
        /// </summary>
        CCirculo[] tempClicksCore;

        /// <summary>
        /// control de cambios
        /// </summary>
        bool changes;

        /// <summary>
        /// Contador para clicks
        /// </summary>
        int countClickCore;

        #endregion

        Point lastClick;        

        public SelectAreasForm()
        {
            InitializeComponent();
        }

        public void CentrarForm()
        {
            //this.Location = new System.Drawing.Point((Screen.PrimaryScreen.WorkingArea.Width - this.Width) / 2, (Screen.PrimaryScreen.WorkingArea.Height - this.Height) / 2);
            this.Location = new System.Drawing.Point((MdiParent.Width - this.Width) / 2, (int)((MdiParent.Height - this.Height) *0.1/ 2));
        }

        private void label4_DoubleClick(object sender, EventArgs e)
        {
            CentrarForm();            
        }

        private void label4_MouseDown(object sender, MouseEventArgs e)
        {
            lastClick = e.Location;
        }

        private void label4_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.Left += e.X - lastClick.X;
                this.Top += e.Y - lastClick.Y;
            }
        }

        private void btnCerrar_MouseEnter(object sender, EventArgs e)
        {
            ((System.Windows.Forms.Button)(sender)).ForeColor = Color.White;
        }

        private void btnCerrar_MouseLeave(object sender, EventArgs e)
        {
            ((System.Windows.Forms.Button)(sender)).ForeColor = Color.Black;
        }

        private void btnCerrar_Click(object sender, EventArgs e)
        {
            this.padre.CerrarSelectAreasForm();
        }

        private void SelectAreasForm_Load(object sender, EventArgs e)
        {
            tempClicksCore= new CCirculo[3];
            countClickCore=0;
            
            // lapiz
            float[] dashValues = { 10, 3, 5, 3 };
            pen2 = new System.Drawing.Pen(Color.LawnGreen, 2F);
            pen2.DashPattern = dashValues;
            
            // control de cambios
            changes = true;

            // se prepara la barra
            this.trackElementos.Minimum = 1;
            this.trackElementos.Maximum = padre.actual.datacuboHigh.dataCube.Count;
            this.trackElementos.TickFrequency = (int)(padre.actual.datacuboHigh.dataCube.Count / 10);
            this.trackElementos.Value = 1;

            this.pictCore.Image = MyDicom.CrearBitmap(padre.actual.datacuboHigh.dataCube[0].segCore, padre.actual.areaCore.width * 2, padre.actual.areaCore.width * 2);

            // si existe informacion de phantoms en los DICOM
            if (padre.actual.phantomEnDicom)
            {
                //this.pictP1.Image = MainForm.Byte2image(elementosP1[0]);
                //this.pictP2.Image = MainForm.Byte2image(elementosP2[0]);
                //this.pictP3.Image = MainForm.Byte2image(elementosP3[0]);
            }
            else
            {
                // no hay informacion de phantoms en dicom
                grpPhantoms.Enabled = false;
            }

            // se prepara una lista vacia de areas para los core, cada una con elementos null
            areasCore=new List<CCuadrado>();
            for (int i = 0; i < padre.actual.datacuboHigh.dataCube.Count; i++) areasCore.Add(null);

            numUntil.Value = padre.actual.datacuboHigh.dataCube.Count;
        }

        private void trackElementos_ValueChanged(object sender, EventArgs e)
        {
            int n = trackElementos.Value;

            if(changes)
            {
                changes = false;
                
                numActual.Value = n;

                changes = true;
            }

            Filtrar(n);

            // si existe informacion de phantoms en los DICOM
            if (padre.actual.phantomEnDicom)
            {
                //this.pictP1.Image = MainForm.Byte2image(elementosP1[n - 1]);
                //this.pictP2.Image = MainForm.Byte2image(elementosP2[n - 1]);
                //this.pictP3.Image = MainForm.Byte2image(elementosP3[n - 1]);
            }

            pictCore.Invalidate();
        }

        private void numActual_ValueChanged(object sender, EventArgs e)
        {
            if(changes)
            {
                changes = false;

                trackElementos.Value = Convert.ToInt32(numActual.Value);

                changes = true;
            }
        }

        private void rangeBar_RangeChanged(object sender, EventArgs e)
        {
            
        }

        /// <summary>
        /// Se calcula el nuevo elemento usando los 3-clicks realizados sobre la imagen, y se agrega al List
        /// </summary>
        public void AddElemento(CCirculo[] tempClicks, string elemento)
        {
            // tomado del script calcCircle.m

            CCuadrado punto = new CCuadrado();

            double epsilon = 0.000000001;

            bool ax_is_0 = (Math.Abs(tempClicks[1].x - tempClicks[0].x) <= epsilon);
            bool bx_is_0 = (Math.Abs(tempClicks[2].x - tempClicks[1].x) <= epsilon);

            // check whether both lines are vertical - collinear
            if (ax_is_0 && bx_is_0)
            {
                MessageBox.Show("Los puntos ingresados pertenecen a una misma linea recta", "Error al dibujar!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int delta_a_x = tempClicks[1].x - tempClicks[0].x;
            int delta_a_y = tempClicks[1].y - tempClicks[0].y;
            int delta_b_x = tempClicks[2].x - tempClicks[1].x;
            int delta_b_y = tempClicks[2].y - tempClicks[1].y;

            // make sure delta gradients are not vertical
            // swap points to change deltas
            if (ax_is_0)
            {
                int temp;
                temp = tempClicks[1].x;
                tempClicks[1].x = tempClicks[2].x;
                tempClicks[2].x = temp;
                temp = tempClicks[1].y;
                tempClicks[1].y = tempClicks[2].y;
                tempClicks[2].y = temp;
                delta_a_x = tempClicks[1].x - tempClicks[0].x;
                delta_a_y = tempClicks[1].y - tempClicks[0].y;
            }

            if (bx_is_0)
            {
                int temp;
                temp = tempClicks[0].x;
                tempClicks[0].x = tempClicks[1].x;
                tempClicks[1].x = temp;
                temp = tempClicks[0].y;
                tempClicks[0].y = tempClicks[1].y;
                tempClicks[1].y = temp;
                delta_b_x = tempClicks[2].x - tempClicks[1].x;
                delta_b_y = tempClicks[2].y - tempClicks[1].y;
            }

            double grad_a = (double)delta_a_y / (double)delta_a_x;
            double grad_b = (double)delta_b_y / (double)delta_b_x;

            // check whether the given points are collinear
            if (Math.Abs(grad_a - grad_b) <= epsilon)
            {
                MessageBox.Show("Los puntos ingresados pertenecen a una misma linea recta", "Error al dibujar!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // swap grads and points if grad_a is 0
            if (Math.Abs(grad_a) <= epsilon)
            {
                double temp2;
                temp2 = grad_a;
                grad_a = grad_b;
                grad_b = temp2;
                int temp;
                temp = tempClicks[0].x;
                tempClicks[0].x = tempClicks[2].x;
                tempClicks[2].x = temp;
                temp = tempClicks[0].y;
                tempClicks[0].y = tempClicks[2].y;
                tempClicks[2].y = temp;
            }

            // calculate centre - where the lines perpendicular to the centre of segments a and b intersect
            punto.x = Convert.ToInt32((grad_a * grad_b * (tempClicks[0].y - tempClicks[2].y) + grad_b * (tempClicks[0].x + tempClicks[1].x) - grad_a * (tempClicks[1].x + tempClicks[2].x)) / (2 * (grad_b - grad_a)));
            punto.y = Convert.ToInt32(((tempClicks[0].x + tempClicks[1].x) / 2 - punto.x) / grad_a + (tempClicks[0].y + tempClicks[1].y) / 2);
            punto.width = Convert.ToInt32(Math.Sqrt(Math.Pow(punto.x - tempClicks[0].x, 2) + Math.Pow(punto.y - tempClicks[0].y, 2)));
            
            switch(elemento)
            {
                case "core":
                    // se agrega ese elemento area TODOS los elementos de ahi en adelante
                    for (int i = trackElementos.Value - 1; i < padre.actual.datacuboHigh.dataCube.Count;i++) this.areasCore[i] = punto;
                    controlPaint = true;
                    pictCore.Invalidate();
                    padre.selecAreas2Form.GetAreasCore(areasCore, pictCore.Image.Width);
                    break;
            }

            btnClear.Enabled = true;
            btnDelete.Enabled = true;
        }

        private void pictCore_MouseClick(object sender, MouseEventArgs e)
        {
            // solo se cuentan los clicks en modo manual
            // se asegura que el PictureBox obtenga el foco
            pictCore.Focus();

            // se guarda la coordenada del click, y se aumenta el contador
            tempClicksCore[countClickCore] = new CCirculo(e.X, e.Y, 0);
            countClickCore++;

            switch (countClickCore)
            {
                case 1:
                    lblPunto1.Visible = true;
                    btnCancel.Enabled = true;
                    break;
                case 2:
                    lblPunto2.Visible = true;
                    break;
                case 3:
                    // con tres clicks se dibuja el circulo
                    AddElemento(tempClicksCore,"core");
                    ResetCountClickCore();
                    break;
            }
        }

        private void pictCore_MouseEnter(object sender, EventArgs e)
        {
            // se cambia el cursor si se ha definido el MODO MANUAL
            this.Cursor = Cursors.Cross;
            
        }

        private void pictCore_MouseLeave(object sender, EventArgs e)
        {
            this.Cursor = Cursors.Default;
        }

        /// <summary>
        /// Se resetea el conteo de clicks
        /// </summary>
        public void ResetCountClickCore()
        {
            countClickCore = 0;
            tempClicksCore = new CCirculo[3];

            lblPunto1.Visible = false;
            lblPunto2.Visible = false;
            btnCancel.Enabled = false;
        }

        private void pictCore_Paint(object sender, PaintEventArgs e)
        {
            int i = trackElementos.Value-1;
            if (areasCore[i] != null)
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                e.Graphics.DrawEllipse(pen2, areasCore[i].x - areasCore[i].width, areasCore[i].y - areasCore[i].width, 2 * areasCore[i].width, 2 * areasCore[i].width);
            }      
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void SelectAreasForm_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawRectangle(new Pen(Color.Green, 2), this.DisplayRectangle);       
        }

        private void groupBox3_Enter(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// Modifica el brillo y contraste de la imagen que aparece en pantalla
        /// </summary>
        private void Filtrar(int n)
        {
            Bitmap temp = MyDicom.CrearBitmap(padre.actual.datacuboHigh.dataCube[n - 1].segCore, padre.actual.areaCore.width * 2, padre.actual.areaCore.width * 2);
            
            AForge.Imaging.Filters.BrightnessCorrection brillo = new AForge.Imaging.Filters.BrightnessCorrection(Convert.ToInt32(trackBrillo.Value));
            AForge.Imaging.Filters.ContrastCorrection contraste = new AForge.Imaging.Filters.ContrastCorrection(Convert.ToInt32(trackContraste.Value));

            contraste.ApplyInPlace(temp);
            brillo.ApplyInPlace(temp);

            pictCore.Image = temp;

            //pictElemento.Invalidate();
        }

        private void trackBrillo_Scroll(object sender, EventArgs e)
        {
            int n = trackElementos.Value;
            Filtrar(n);
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            int n = trackElementos.Value;
            trackBrillo.Value = 0;
            trackContraste.Value = 0;

            Filtrar(n);
        }
    }
}
