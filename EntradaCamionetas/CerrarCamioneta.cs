using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.Data.SqlClient;
using System.Data;
using Android.Text;

namespace EntradaCamionetas
{
    [Activity(Label = "Cerrar Camioneta")]
    public partial class CerrarCamioneta : Activity
    {

        SqlConnection thisConnection = new SqlConnection(MainActivity.cadenaConexion);
        SqlDataAdapter da;
        DataSet ds = new DataSet();
        SqlCommand cmnd = new SqlCommand();
        public static DataTable vehiculos = new DataTable("vehiculos");
        String[] strFrutas;

        Spinner Vehiculos;
        EditText TempFinal;
        TextView fechaFinal;

        ArrayAdapter<String> comboAdapter;

        Button Guardar;

        public static string vehiculo = "";


        string query = "";
        
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.cerrarcamioneta);


            Guardar = FindViewById<Button>(Resource.Id.btnbclose);
            Guardar.Click += BtnGuardar_Click;

            ActionBar.NavigationMode = ActionBarNavigationMode.Tabs;


            ActionBar.Tab tab = ActionBar.NewTab();
            tab.SetText(Resources.GetString(Resource.String.tab1_text));
            tab.SetIcon(Resource.Drawable.registro);
            tab.TabSelected += (sender, args) => {
                Intent intent = new Intent(this, typeof(MainActivity));
                intent.AddFlags(ActivityFlags.ClearTop);
                intent.AddFlags(ActivityFlags.SingleTop);
                StartActivity(intent);
                Finish();
            };
            ActionBar.AddTab(tab, 0, false);

            tab = ActionBar.NewTab();
            tab.SetText(Resources.GetString(Resource.String.tab2_text));
            tab.SetIcon(Resource.Drawable.pedido);
            tab.TabSelected += (sender, args) => {
                Intent intent2 = new Intent(this, typeof(Asignar_Pedidos));
                intent2.AddFlags(ActivityFlags.ClearTop);
                intent2.AddFlags(ActivityFlags.SingleTop);
                StartActivity(intent2);
                Finish();
            };
            ActionBar.AddTab(tab, 1, false);
             
            tab = ActionBar.NewTab();
            tab.SetText(Resources.GetString(Resource.String.tab3_text));
            tab.SetIcon(Resource.Drawable.salir);
            tab.TabSelected += (sender, args) => {
                // Do something when tab is selected
            };
            ActionBar.AddTab(tab, 2, true);


            Vehiculos = FindViewById<Spinner>(Resource.Id.spinner7);
            TempFinal = FindViewById<EditText>(Resource.Id.TempFin);
            fechaFinal = FindViewById<TextView>(Resource.Id.fechafin);


            //Llenado de spinner Responsable ***********************************************************************************************************
            thisConnection.Open();
            query = "select * FROM tb_mstr_trailer LEFT JOIN tb_cat_vehiculos ON no_trailer = clave WHERE transporte = 'PC' AND tempfin = '' AND Guardar = 'N' AND clave != ''  AND horafin = '--:--'";
            da = new SqlDataAdapter(query, thisConnection);
            da.Fill(ds, "vehiculos");
            vehiculos = ds.Tables["vehiculos"];
            thisConnection.Close();


            System.Collections.ArrayList listaFrutas2 = new System.Collections.ArrayList();

            strFrutas = new String[vehiculos.Rows.Count + 1];
            strFrutas[0] = "Seleccione un Vehiculo";
            for (int i = 1; i <= vehiculos.Rows.Count; i++)
            {
                int x = i - 1;
                strFrutas[i] = vehiculos.Rows[x]["clave"].ToString();
            }

            comboAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, strFrutas);
            Vehiculos.Adapter = comboAdapter;
            Vehiculos.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(spinner_Item_Vehiculos);


            TempFinal.AfterTextChanged += (object sender, AfterTextChangedEventArgs e) =>
            {
                var ampmx = System.DateTime.Now.ToString("tt");
                ampmx = ampmx.Replace(" ", "");
                fechaFinal.Text = System.DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss ") + ampmx;
            };

        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            if (vehiculo == "Seleccione un Vehiculo")
            {
                Toast.MakeText(this, "Por favor, asegurese de seleccionar un vehiculo", ToastLength.Long).Show();
                return;
            }
            else if (TempFinal.Text == "")
            {
                Toast.MakeText(this, "Por favor, asegurese de Ingresar una Temperatura Final", ToastLength.Long).Show();
                return;
            }
            else {
                int falta = 0;
                string peddidosfaltantes = "";
                DataTable CatProd = new DataTable();
                DataTable CatProdu = new DataTable();
                thisConnection.Open();
                //string cadena = "SELECT * FROM  tb_mstr_facturas_nal A INNER JOIN tb_mstr_pedidos_nal B ON A.pdn_folio = B.pdn_folio Where B.pdn_surtido != 'S' AND B.pdn_estatus != 'C' AND A.prov_clave = 'MRLUCKY'  AND A.cve_auto = '" + vehiculo.Trim() +"' AND fcn_fecha > '13/02/2019' Order by fcn_fecha DESC";
                string cadena = "SELECT A.pdn_folio FROM  tb_mstr_facturas_nal A Where A.prov_clave = 'MRLUCKY'  AND A.cve_auto = '" + vehiculo.Trim() + "' AND fcn_fecha > '13/02/2019' AND pdn_folio IN (SELECT pdn_folio FROM tb_mstr_pedidos_nal Where  pdn_surtido != 'S' AND pdn_estatus != 'C') Order by fcn_fecha DESC";
                SqlDataAdapter da = new SqlDataAdapter(cadena, thisConnection);
                DataSet ds = new DataSet();
                da.Fill(ds, "CatProd");
                CatProd = ds.Tables["CatProd"];
                thisConnection.Close();
                foreach (DataRow Row in CatProd.Rows) {
                    
                    falta++;
                    peddidosfaltantes += Row["pdn_folio"].ToString().Trim() + "\n\r";
                    
                }


                thisConnection.Open();
                //string cadena = "SELECT * FROM  tb_mstr_facturas_nal A INNER JOIN tb_mstr_pedidos_nal B ON A.pdn_folio = B.pdn_folio Where B.pdn_surtido != 'S' AND B.pdn_estatus != 'C' AND A.prov_clave = 'MRLUCKY'  AND A.cve_auto = '" + vehiculo.Trim() +"' AND fcn_fecha > '13/02/2019' Order by fcn_fecha DESC";
                cadena = "SELECT DISTINCT(emb_folio) AS pdn_folio FROM tb_det_split WHERE(estatus = 'A') AND (emb_folio IN (SELECT DISTINCT emb_folio FROM Tb_Det_Etiqueta WHERE(Cve_Camioneta = '" + vehiculo.Trim() + "'))) AND(emb_folio NOT IN(SELECT A.pdn_folio FROM  tb_mstr_facturas_nal A Where A.prov_clave = 'MRLUCKY'  AND A.cve_auto = '" + vehiculo.Trim() + "' AND fcn_fecha > '13/02/2019' AND pdn_folio IN (SELECT pdn_folio FROM tb_mstr_pedidos_nal Where  pdn_surtido != 'S' AND pdn_estatus != 'C')))";
                SqlDataAdapter das = new SqlDataAdapter(cadena, thisConnection);
                DataSet dsi = new DataSet();
                das.Fill(dsi, "CatProdu");
                CatProdu = dsi.Tables["CatProdu"];
                thisConnection.Close();
                foreach (DataRow Row in CatProdu.Rows)
                {

                    falta++;
                    peddidosfaltantes += Row["pdn_folio"].ToString().Trim() + "\n\r";

                }





                if (falta == 0)
                {
                    thisConnection.Open();
                    cadena = "UPDATE tb_mstr_trailer SET tempfin = '" + TempFinal.Text + "', horafin = '" + fechaFinal.Text.Trim() + "', Guardar = 'S'  WHERE no_trailer = '" + vehiculo + "' AND transporte = 'PC' AND tempfin = '' AND Guardar = 'N'  AND horafin = '--:--'";
                    SqlCommand cmd = new SqlCommand(cadena, thisConnection);
                    cmd.ExecuteNonQuery();
                    thisConnection.Close();

                    Android.App.AlertDialog.Builder alertDialog = new Android.App.AlertDialog.Builder(this);
                    alertDialog.SetTitle(Html.FromHtml("<font color='#DF0101' size = 10>Vehiculo Cerrado Correctamente</font>"));
                    alertDialog.SetIcon(Resource.Drawable.exito);
                    alertDialog.SetMessage(Html.FromHtml("<font color='#FFFFFF' size = 10>El vehiculo " + vehiculo + " fue Cerrado Correctamente</font>"));
                    alertDialog.SetCancelable(false);
                    alertDialog.SetNeutralButton("Ok", delegate
                    {
                        thisConnection.Open();
                        query = "select * FROM tb_mstr_trailer LEFT JOIN tb_cat_vehiculos ON no_trailer = clave WHERE transporte = 'PC' AND tempfin = '' AND Guardar = 'N' AND clave != ''";
                        da = new SqlDataAdapter(query, thisConnection);
                        da.Fill(ds, "vehiculos");
                        vehiculos = ds.Tables["vehiculos"];
                        thisConnection.Close();


                        System.Collections.ArrayList listaFrutas2 = new System.Collections.ArrayList();

                        strFrutas = new String[vehiculos.Rows.Count + 1];
                        strFrutas[0] = "Seleccione un Vehiculo";
                        for (int i = 1; i <= vehiculos.Rows.Count; i++)
                        {
                            int x = i - 1;
                            strFrutas[i] = vehiculos.Rows[x]["clave"].ToString();
                        }

                        comboAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, strFrutas);
                        Vehiculos.Adapter = comboAdapter;
                        Vehiculos.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(spinner_Item_Vehiculos);
                        TempFinal.Text = "";
                        fechaFinal.Text = "Fecha: 00/00/0000 00:00:00 *.M";
                    });

                    alertDialog.Show();

                }
                else {
                    View view = LayoutInflater.Inflate(Resource.Layout.listapedidospendientes, null);
                    AlertDialog.Builder builder = new AlertDialog.Builder(this);
                    builder.SetTitle(Html.FromHtml("<font color='#FF8A34' size = 10>Pedidos Faltantes por Cargar</font>"));
                    builder.SetView(view);
                    TextView titulo = view.FindViewById<TextView>(Resource.Id.about_app);
                    titulo.Text = "El vehiculo: " + vehiculo.Trim() + " Tiene pedidos pendientes por cargar: \n "+ peddidosfaltantes;
                    builder.SetNeutralButton("Ok", delegate
                    {
                        builder.Dispose();
                    });
                    builder.Show();



                /*LayoutInflater inflater = LayoutInflater.From(this);
                    View viewer = inflater.Inflate(Resource.Layout.listapedidospendientes, null);
                    TextView textview = (TextView)viewer.FindViewById<TextView>(Resource.Id.); ;
                    textview.SetText("Your really long message.");
                    Android.App.AlertDialog.Builder alertDialog = new Android.App.AlertDialog.Builder(this);
                    alertDialog.SetTitle(Html.FromHtml("<font color='#FF8A34' size = 10>Pedidos Faltantes por Cargar</font>"));
                    alertDialog.SetIcon(Resource.Drawable.warning);
                    alertDialog.SetMessage(Html.FromHtml("<font color='#FFFFFF' size = 10>El vehiculo: " + vehiculo.Trim() + " Tiene pedidos pendientes por cargar: \n "+ peddidosfaltantes + " </font>"));
                    alertDialog.sc
                    alertDialog.SetNeutralButton("Ok", delegate
                    {
                        alertDialog.Dispose();
                    });
                    alertDialog.Show();*/
                }
                
            }
        }

        private void spinner_Item_Vehiculos(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            Spinner spinner = (Spinner)sender;
            vehiculo = spinner.GetItemAtPosition(e.Position).ToString();
        }
    }
}

