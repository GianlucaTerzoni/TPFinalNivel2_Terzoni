﻿using Negocio;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Negocio;
using Dominio;
using System.Security.AccessControl;
using System.Globalization;
using Microsoft.VisualBasic;
using Microsoft.SqlServer.Server;

namespace Presentacion
{
    public partial class Form1 : Form
    {

        private List<Articulo> listaArticulos;
        public Form1()
        {
            InitializeComponent();

        }


        private void Form1_Load(object sender, EventArgs e)
        {
            Cargar();

            cboCampo.Items.Add("Precio");
            cboCampo.Items.Add("Marca");
            cboCampo.Items.Add("Categoría");

            DataGridViewColumn columnaPrecio = dgvArticulos.Columns["Precio"];


            if (columnaPrecio != null)
            {

                columnaPrecio.DefaultCellStyle.Format = "#,0.##";
            }

        }

        private void dgvArticulos_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvArticulos.CurrentRow != null)
            {
                Articulo seleccionado = (Articulo)dgvArticulos.CurrentRow.DataBoundItem;
                CargarImagen(seleccionado.Imagen);
            }

        }


        private void Cargar()
        {

            ArticuloNegocio negocio = new ArticuloNegocio();

            try
            {
                listaArticulos = negocio.Listar();
                foreach (var articulo in listaArticulos)
                {
                    articulo.Precio = Math.Round(articulo.Precio, 2);

                }


                dgvArticulos.DataSource = listaArticulos;
                
                OcultarColumnas();
                CargarImagen(listaArticulos[0].Imagen);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

        }

        private void CargarImagen(string imagen)
        {
            try
            {
                pbxImagenArticulo.Load(imagen);
            }
            catch (Exception)
            {

                pbxImagenArticulo.Load("https://editorial.unc.edu.ar/wp-content/uploads/sites/33/2022/09/placeholder.png");
            }
        }

        private void OcultarColumnas()
        {
            dgvArticulos.Columns["Imagen"].Visible = false;
            dgvArticulos.Columns["Id"].Visible = false;
            


        }

        private void btnAgregar_Click(object sender, EventArgs e)
        {
            FormularioAlta alta = new FormularioAlta();
            alta.ShowDialog();
            Cargar();
        }

        private void btnModificar_Click(object sender, EventArgs e)
        {
            Articulo seleccionado;
            seleccionado = (Articulo)dgvArticulos.CurrentRow.DataBoundItem;

            FormularioAlta modificar = new FormularioAlta(seleccionado);
            modificar.ShowDialog();
            Cargar();

        }

        private void btnEliminar_Click(object sender, EventArgs e)
        {
            Eliminar();             
        }

        private void Eliminar()
        {
            ArticuloNegocio negocio = new ArticuloNegocio();
            Articulo seleccionado;

            try
            {

                DialogResult respuesta = MessageBox.Show("¿Está seguro de eliminarlo?", "Eliminando", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if(respuesta == DialogResult.Yes)
                {
                    seleccionado = (Articulo)dgvArticulos.CurrentRow.DataBoundItem;

                    negocio.Eliminar(seleccionado);

                    Cargar();
                }

            }
            catch (Exception)
            {

                MessageBox.Show("Error al intentar eliminar el articulo.");
            }
        }

        private void txtBuscar_TextChanged(object sender, EventArgs e)
        {
            List<Articulo> listaFiltrada;
            string filtro = txtBuscar.Text;

            if (filtro.Length >= 2)
            {
                listaFiltrada = listaArticulos.FindAll(i => i.Nombre.ToUpper().Contains(filtro.ToUpper()) || i.Marca.Descripcion.ToUpper().Contains(filtro.ToUpper()));
            }
            else
            {
                listaFiltrada = listaArticulos;
            }
            dgvArticulos.DataSource = null;
            dgvArticulos.DataSource = listaFiltrada;
            OcultarColumnas();
        }

        private void cboCampo_SelectedIndexChanged(object sender, EventArgs e)
        {
            string opcion = cboCampo.SelectedItem.ToString();
            

            if (opcion == "Precio")
            {
                cboCriterio.Items.Clear();
                cboCriterio.Items.Add("Menor a");
                cboCriterio.Items.Add("Mayor a");
                cboCriterio.Items.Add("Igual a");
                
            }
            else
            {
                cboCriterio.Items.Clear();
                cboCriterio.Items.Add("Comienza con");
                cboCriterio.Items.Add("Termina con");
                cboCriterio.Items.Add("Contiene");
                
            }        

        }

        private bool ValidarFiltroAvanzado()
        {

            if (cboCampo.SelectedIndex < 0)
            {
                MessageBox.Show("Por favor, seleccione el campo.");
                return true;
            }

            if(cboCriterio.SelectedIndex < 0)
            {
                MessageBox.Show("Por favor, seleccione el criterio.");
                return true;
            }

            if(cboCampo.Text == "Precio")
            {
                if (string.IsNullOrEmpty(txtFiltro.Text))
                {
                    MessageBox.Show("Debe ingresar al menos un valor numérico. ");
                    return true;
                }

                if (!(SoloNumeros(txtFiltro.Text)))
                {
                    MessageBox.Show("Debe ingresar solo valores numéricos.");
                    txtFiltro.Text = "";
                    return true;
                }

            }

            return false;
        }

        private bool SoloNumeros(string cadena)
        {
            foreach (char caracter in cadena)
            {
                if (!(char.IsNumber(caracter)))

                    return false;

            }

            decimal number;
            return Decimal.TryParse(cadena, NumberStyles.Any, CultureInfo.InvariantCulture, out number);
            
        }

        private void btnFiltro_Click(object sender, EventArgs e)
        {
            ArticuloNegocio negocio = new ArticuloNegocio();


            try
            {
                if (ValidarFiltroAvanzado())
                    return;

                string campo = cboCampo.SelectedItem.ToString();
                string criterio = cboCriterio.SelectedItem.ToString();
                string filtro = txtFiltro.Text;
                
                dgvArticulos.DataSource = negocio.Filtrar(campo, criterio, filtro);


                DataGridViewColumn columnaPrecio = dgvArticulos.Columns["Precio"];


                if (columnaPrecio != null)
                {

                    columnaPrecio.DefaultCellStyle.Format = "#,0.##";
                }
            }
            
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void btnDetalle_Click(object sender, EventArgs e)
        {


            if (dgvArticulos.CurrentRow != null)
            {
                Articulo seleccionado;
                seleccionado = (Articulo)dgvArticulos.CurrentRow.DataBoundItem;
                FormDetalle detalle = new FormDetalle(seleccionado);
                detalle.ShowDialog();
                Cargar();
                
            }
        }

        private void btnCargarListaCompleta_Click(object sender, EventArgs e)
        {
            Cargar();
        }

        private void btnAgregarMarca_Click(object sender, EventArgs e)
        {
            FormAgregarMarca agregarMarca = new FormAgregarMarca();
            agregarMarca.ShowDialog();
            Cargar();
        }

        private void btnAgregarCategoria_Click(object sender, EventArgs e)
        {
            AgregarCategoria categoria = new AgregarCategoria();
            categoria.ShowDialog();
            Cargar();
        }

        private void btnEliminarMarca_Click(object sender, EventArgs e)
        {
            FormEliminarMarca borrarMarca = new FormEliminarMarca();
            borrarMarca.ShowDialog();
            Cargar();
        }

        private void btnEliminarCategoria_Click(object sender, EventArgs e)
        {
            FormEliminarCategoria borrarCategoria = new FormEliminarCategoria();
            borrarCategoria.ShowDialog();
            Cargar();
        }
    }
}
