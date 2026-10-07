using Android.App;
using Android.Widget;
using Android.OS;
using Android.Content;
using System;
using System.Data;
using System.Data.SqlClient;
using Java.Util;
using Android.Text;
using System.Collections.Generic;
using Android.Net.Wifi;
using System.Net;

namespace EntradaCamionetas
{
    [Activity(Label = "Captura Camionetas", MainLauncher = true)]
    public class MainActivity : Activity
    {

        public static string cadenaConexion = "Persist Security Info=False;user id=sa; password=Gabira2026$;Initial Catalog =GAB_Irapuato; server=tcp:189.206.160.206,2352; Connect Timeout = 130";
        //public static string cadenaConexion = "Persist Security Info=False;user id=sa; password=Gabira2026$;Initial Catalog =GAB_Irapuato; server=tcp:192.168.123.6,1433; Connect Timeout = 130";
        public static string veh = "";
        public static int captura = 0;
        SqlCommand cmnd = new SqlCommand();
        SqlDataReader reader;
        SqlCommand cmnd1 = new SqlCommand();
        SqlDataReader reader1;
        String[] strFrutas;
        String[] strTurno;
        String[] stranden;
        String[] strchofer;
        String[] strresponsable;
        String[] strdestino;
        ArrayAdapter<String> comboAdapter;
        ArrayAdapter<String> comboAdapter2;
        ArrayAdapter<String> comboAdapter3;
        ArrayAdapter<String> comboAdapter4;
        ArrayAdapter<String> comboAdapter5;
        ArrayAdapter<String> comboAdapterdestino;


        SqlDataAdapter da;
        SqlDataAdapter da1;
        public static DataTable camionetas = new DataTable("camionetas");
        public static DataTable responsables = new DataTable("responsables");
        public static DataTable choferes = new DataTable("choferes");
        public static DataTable vehiculos = new DataTable("vehiculos");
        public static DataTable version = new DataTable("version");
        public static DataTable formulario = new DataTable("formulario");
        string query = "";
        DataSet ds = new DataSet();
        DataSet ds1 = new DataSet();
        public static string vehiculo = "";
        public static string turno = "";
        public static string destino = "";
        public static string anden = "";
        public static string chofer = "";
        public static string responsable = "";
        public static string responsablesplit = "";
        public static string imei = "";
        public static string ip = "";
        SqlConnection thisConnection;

        public static DataTable pedidosx = new DataTable("pedidos");


        TextView fechaini;
        Spinner Vehiculos;
        Spinner Turno;
        Spinner Anden;
        Spinner Destino;
        Spinner Chofer;
        Spinner Responsable;
        EditText TempIni;
        TextView fechaIni;
        Button Guardar;


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Main);

            ActionBar.NavigationMode = ActionBarNavigationMode.Tabs;

            ActionBar.Tab tab = ActionBar.NewTab();
            tab.SetText(Resources.GetString(Resource.String.tab1_text));
            tab.SetIcon(Resource.Drawable.registro);
            tab.TabSelected += (sender, args) =>
            {
                // Do something when tab is selected
            };
            ActionBar.AddTab(tab, 0, true);

            tab = ActionBar.NewTab();
            tab.SetText(Resources.GetString(Resource.String.tab2_text));
            tab.SetIcon(Resource.Drawable.pedido);
            tab.TabSelected += (sender, args) =>
            {
                Intent intent = new Intent(this, typeof(Asignar_Pedidos));
                intent.AddFlags(ActivityFlags.ClearTop);
                intent.AddFlags(ActivityFlags.SingleTop);
                StartActivity(intent);
                Finish();
            };
            ActionBar.AddTab(tab, 1, false);

            tab = ActionBar.NewTab();
            tab.SetText(Resources.GetString(Resource.String.tab3_text));
            tab.SetIcon(Resource.Drawable.salir);
            tab.TabSelected += (sender, args) =>
            {
                Intent intent2 = new Intent(this, typeof(CerrarCamioneta));
                intent2.AddFlags(ActivityFlags.ClearTop);
                intent2.AddFlags(ActivityFlags.SingleTop);
                StartActivity(intent2);
                Finish();
            };
            ActionBar.AddTab(tab, 2, false);


            //obtener Ip del telefono
            WifiManager wifiManager = (WifiManager)this.GetSystemService(Service.WifiService);
            ip = GetIPAddress();
            //obtener Imei del telefono
            imei = Android.Provider.Settings.Secure.GetString(ContentResolver, Android.Provider.Settings.Secure.AndroidId);
            //Termina obtener datos

            thisConnection = new SqlConnection(cadenaConexion);

            //Registro de Ingreso Al Sistema (en segundo plano, con su propia conexion, para no bloquear la carga de la pantalla).
            string cadena = "INSERT INTO TB_REGISTRO_MOVIMIENTOS(FECHA,NOM_COMPU,NOM_USU,TIPO_MOV,OP_CLAVE,FOLIO,DETALLE,SISTEMA,MOV_FOLIO) " +
                        "VALUES('" + System.DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "','CEL " + imei + "','CAPTURA CAMIONETA','E','" + ip + "','','Ingreso a sistema Captura Camioneta Imei: " + imei + ", Ip: " + ip + " ','CAPCAM','')";
            System.Threading.ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    using (var conReg = new SqlConnection(cadenaConexion))
                    {
                        conReg.Open();
                        using (var cmdReg = new SqlCommand(cadena, conReg))
                        {
                            cmdReg.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception)
                {
                    // El registro de bitacora no debe impedir el uso de la aplicacion.
                }
            });


            fechaini = FindViewById<TextView>(Resource.Id.fechacaptura);
            Vehiculos = FindViewById<Spinner>(Resource.Id.spinner1);
            Turno = FindViewById<Spinner>(Resource.Id.spinner2);
            Anden = FindViewById<Spinner>(Resource.Id.spinner3);
            Destino = FindViewById<Spinner>(Resource.Id.Destino);
            Chofer = FindViewById<Spinner>(Resource.Id.spinner4);
            Responsable = FindViewById<Spinner>(Resource.Id.spinnerx4);
            TempIni = FindViewById<EditText>(Resource.Id.TempIni);
            fechaIni = FindViewById<TextView>(Resource.Id.fechaInicioca);

            Guardar = FindViewById<Button>(Resource.Id.btnlogin);
            Guardar.Click += BtnGuardar_Click;


            //llenado de Fecha y hora de inicio
            var ampm = System.DateTime.Now.ToString("tt");
            ampm = ampm.Replace(" ", "");
            fechaini.Text = System.DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss ") + ampm;

            //Llenado de spinner vehiculos ***********************************************************************************************************
            // Una sola conexion abierta para todas las cargas iniciales de catalogos.
            thisConnection.Open();
            query = "select clave FROM tb_cat_vehiculos Where estatus = 'A' AND clave NOT IN (SELECT no_trailer FROM tb_mstr_trailer WHERE horafin = '--:--' AND no_trailer = clave AND transporte = 'PC' AND tempfin = '' AND Guardar = 'N')  ORDER BY clave";
            da = new SqlDataAdapter(query, thisConnection);
            da.Fill(ds, "vehiculos");
            vehiculos = ds.Tables["vehiculos"];

            List<string> list = new List<string>();

            list.Add("Seleccione un Vehiculo");

            var claves = new List<string>(vehiculos.Rows.Count);
            foreach (DataRow row in vehiculos.Rows)
            {
                claves.Add(row["clave"].ToString());
            }

            // Pedidos pendientes de todas las camionetas en pocos viajes al servidor (antes: 2 consultas por camioneta).
            int[] pendientes = ContarPendientes(claves);
            for (int i = 0; i < claves.Count; i++)
            {
                if (pendientes[i] > 0)
                {
                    list.Add(claves[i]);
                }
            }

            strFrutas = list.ToArray();

            System.Collections.ArrayList listaFrutas2 = new System.Collections.ArrayList();


            Collections.AddAll(listaFrutas2, strFrutas);
            comboAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, strFrutas);
            Vehiculos.Adapter = comboAdapter;
            Vehiculos.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(spinner_Item_Vehiculos);


            //Llenado de spinner Turno **************************************************************************** //

            strTurno = new String[3];
            strTurno[0] = "Seleccione un Turno";
            strTurno[1] = "1";
            strTurno[2] = "2";

            comboAdapter2 = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, strTurno);
            Turno.Adapter = comboAdapter2;
            Turno.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(spinner_Item_Turno);

            //Llenado de spinner Destino*****************************************************************************//

            strdestino = new String[13];
            strdestino[0] = "Seleccione un Destino";
            strdestino[1] = "LEON";
            strdestino[2] = "IRAPUATO";
            strdestino[3] = "GUANAJUATO";
            strdestino[4] = "ZACATECAS";
            strdestino[5] = "AGUASCALIENTES";
            strdestino[6] = "ZAMORA";
            strdestino[7] = "MORELIA";
            strdestino[8] = "CELAYA";
            strdestino[9] = "QUERETARO JURIQUILLA";
            strdestino[10] = "QUERETARO SAN JUAN DEL RIO";
            strdestino[11] = "SAN LUIS POTOSI";
            strdestino[12] = "DON ARTURO/CENTRAL DE ABASTOS";

            comboAdapterdestino = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, strdestino);
            Destino.Adapter = comboAdapterdestino;
            Destino.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(spinner_Item_Destino);


            //Llenado de spinner Anden **************************************************************************** //

            stranden = new String[15];
            stranden[0] = "Seleccione un Anden";
            stranden[1] = "1";
            stranden[2] = "2";
            stranden[3] = "3";
            stranden[4] = "4";
            stranden[5] = "5";
            stranden[6] = "6";
            stranden[7] = "7";
            stranden[8] = "8";
            stranden[9] = "9";
            stranden[10] = "10";
            stranden[11] = "11";
            stranden[12] = "12";
            stranden[13] = "13";
            stranden[14] = "14";

            comboAdapter3 = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, stranden);
            Anden.Adapter = comboAdapter3;
            Anden.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(spinner_Item_Anden);


            //Llenado de spinner Chofer ***********************************************************************************************************
            query = "select chofer FROM tb_cat_vehiculos Where estatus = 'A'";
            da = new SqlDataAdapter(query, thisConnection);
            da.Fill(ds, "choferes");
            choferes = ds.Tables["choferes"];



            strchofer = new String[choferes.Rows.Count + 1];
            strchofer[0] = "Seleccione un Chofer";
            for (int i = 1; i <= choferes.Rows.Count; i++)
            {
                int x = i - 1;
                strchofer[i] = choferes.Rows[x]["chofer"].ToString().Trim();
            }


            Collections.AddAll(listaFrutas2, strFrutas);
            comboAdapter4 = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, strchofer);
            Chofer.Adapter = comboAdapter4;
            Chofer.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(spinner_Item_Chofer);


            //Llenado de spinner Responsable ***********************************************************************************************************
            query = "SELECT NOMBRE FROM  TB_RESPONSABLE Where ESTATUS = 'A' And TIPO_EMB = 'C' ORDER BY NOMBRE";
            da = new SqlDataAdapter(query, thisConnection);
            da.Fill(ds, "responsables");
            responsables = ds.Tables["responsables"];
            thisConnection.Close();

            Spinner spinner2 = FindViewById<Spinner>(Resource.Id.spinner2);


            strresponsable = new String[responsables.Rows.Count + 1];
            strresponsable[0] = "Seleccione un Responsable";
            for (int i = 1; i <= responsables.Rows.Count; i++)
            {
                int x = i - 1;
                strresponsable[i] = responsables.Rows[x]["NOMBRE"].ToString();
            }


            //Collections.AddAll(listaFrutas2, strFrutas);
            comboAdapter5 = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, strresponsable);
            Responsable.Adapter = comboAdapter5;
            Responsable.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(spinner_Item_Responsable);


            TempIni.AfterTextChanged += (object sender, AfterTextChangedEventArgs e) =>
            {
                var ampmx = System.DateTime.Now.ToString("tt");
                ampmx = ampmx.Replace(" ", "");
                fechaIni.Text = System.DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss ") + ampmx;
            };

        }

        public string GetIPAddress()
        {
            IPAddress[] adresses = Dns.GetHostAddresses(Dns.GetHostName());

            if (adresses != null && adresses[0] != null)
            {
                return adresses[0].ToString();
            }
            else
            {
                return null;
            }
        }

        private void spinner_Item_Responsable(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            Spinner spinner = (Spinner)sender;
            responsable = spinner.GetItemAtPosition(e.Position).ToString();
        }

        private void spinner_Item_Chofer(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            Spinner spinner = (Spinner)sender;
            chofer = spinner.GetItemAtPosition(e.Position).ToString();
        }

        private void spinner_Item_Anden(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            Spinner spinner = (Spinner)sender;
            anden = spinner.GetItemAtPosition(e.Position).ToString();
        }

        private void spinner_Item_Turno(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            Spinner spinner = (Spinner)sender;
            turno = spinner.GetItemAtPosition(e.Position).ToString();
        }

        private void spinner_Item_Destino(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            Spinner spinner = (Spinner)sender;
            destino = spinner.GetItemAtPosition(e.Position).ToString();
        }

        private void spinner_Item_Vehiculos(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            Spinner spinner = (Spinner)sender;
            vehiculo = spinner.GetItemAtPosition(e.Position).ToString();



            // El texto de seleccion inicial no es una camioneta: la consulta nunca regresa filas.
            if (vehiculo == "Seleccione un Vehiculo")
                return;

            thisConnection.Open();
            string Cadenar = "SELECT TOP (1) horaini FROM tb_mstr_trailer WHERE horafin = '--:--' AND no_trailer = '" + vehiculo.Trim() + "' AND transporte = 'PC' AND tempfin = '' AND Guardar = 'N'";
            SqlDataAdapter da = new SqlDataAdapter(Cadenar, thisConnection);
            DataSet ds = new DataSet();
            da.Fill(ds, "Ped");
            DataTable Ped = ds.Tables["Ped"];

            thisConnection.Close();

            if (Ped.Rows.Count != 0)
            {

                string horainiciox = Ped.Rows[0]["horaini"].ToString();
                Android.App.AlertDialog.Builder alertDialog = new Android.App.AlertDialog.Builder(this);
                alertDialog.SetTitle(Html.FromHtml("<font color='#DF0101' size = 10>Camioneta Abierta</font>"));
                alertDialog.SetIcon(Resource.Drawable.warning);
                alertDialog.SetMessage(Html.FromHtml("<font color='#FFFFFF' size = 10>La Camioneta " + vehiculo + " Esta Abierta desde " + horainiciox + ". No puede Volver a abrirla Hasta finalizar con la carga</font>"));
                alertDialog.SetNeutralButton("Ok", delegate
                {
                    alertDialog.Dispose();
                });
                alertDialog.Create();
                alertDialog.Show();

            }

        }

        private void CancelaAction(object sender, DialogClickEventArgs e)
        {
            return;
        }

        private void SaveAction(object sender, DialogClickEventArgs e)
        {
            var ampmx = System.DateTime.Now.ToString("tt");
            ampmx = ampmx.Replace(" ", "");
            string fechaFinal = System.DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss ") + ampmx;

            thisConnection.Open();
            string cadena = "UPDATE tb_mstr_trailer SET tempfin = '', horafin = '" + fechaFinal.Trim() + "' WHERE no_trailer = '" + vehiculo + "' AND transporte = 'PC' AND tempfin = '' AND Guardar = 'N'";
            SqlCommand cmd = new SqlCommand(cadena, thisConnection);
            cmd.ExecuteNonQuery();

            thisConnection.Close();

            Android.App.AlertDialog.Builder alertDialog = new Android.App.AlertDialog.Builder(this);
            alertDialog.SetTitle(Html.FromHtml("<font color='#DF0101' size = 10>Camioneta cerrada</font>"));
            alertDialog.SetIcon(Resource.Drawable.exito);
            alertDialog.SetMessage(Html.FromHtml("<font color='#FFFFFF' size = 10>Camioneta Cerrada Correctamente!!! </font>"));
            alertDialog.SetCancelable(false);
            alertDialog.SetNeutralButton("Ok", delegate
            {
                alertDialog.Dispose();

            });
            alertDialog.Show();
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            Guardar.Enabled = false;

            if (vehiculo == "Seleccione un Vehiculo")
            {
                Toast.MakeText(this, "Por favor, asegurese de seleccionar un vehiculo", ToastLength.Long).Show();
                return;
            }
            else if (turno == "Seleccione un Turno")
            {
                Toast.MakeText(this, "Por favor, asegurese de seleccionar un Turno", ToastLength.Long).Show();
                return;
            }
            else if (anden == "Seleccione un Anden")
            {
                Toast.MakeText(this, "Por favor, asegurese de seleccionar un Anden", ToastLength.Long).Show();
                return;
            }
            else if (chofer == "Seleccione un Chofer")
            {
                Toast.MakeText(this, "Por favor, asegurese de seleccionar un Chofer", ToastLength.Long).Show();
                return;
            }
            else if (responsable == "Seleccione un Responsable")
            {
                Toast.MakeText(this, "Por favor, asegurese de seleccionar un responsable", ToastLength.Long).Show();
                return;
            }
            else if (destino == "Seleccione un Destino")
            {
                Toast.MakeText(this, "Por favor, asegurese de seleccionar un Destino", ToastLength.Long).Show();
                return;
            }
            else if (TempIni.Text == "")
            {
                Toast.MakeText(this, "Por favor, Es necesario una temperatura inicial", ToastLength.Long).Show();
                return;
            }
            else
            {


                thisConnection.Open();
                string cadena = "insert into  tb_mstr_trailer(fecha, hora_trailer, no_trailer, turno, destino, transporte, tempini, tempfin, horaini, horafin, anden, chofer, responsable, concepto1, concepto2, concepto3, concepto4, concepto5a, concepto5b, concepto5c, concepto5d, concepto5e, concepto5f, concepto5g, concepto5h, concepto5i, concepto5j, concepto5k, concepto5l, concepto6, concepto7, concepto8, concepto9, concepto10, largo, gatas, ryan1, ryan2, posryan1, posryan2, conse, temp, HoraRegVig, HoraEnt, HoraSal, TiempoTot, Radio, Surtible, Peso, Placa, TiempoCar, PesoBascula, ObsTrans, Guardar, Transfer, TempSetPoint) " +
                                    "Values('" + System.DateTime.Now.ToString("dd/MM/yyyy") + "','" + fechaini.Text.Trim() + "','" + vehiculo + "','" + turno + "','" + destino + "','PC','" + TempIni.Text.Trim() + "', '', '" + fechaIni.Text.Trim() + "', '--:--', '" + anden + "','" + chofer + "','" + responsable + "','','','','','','','','','','','','','','','','','','','','','','','0','0','0','0','0','0','0','','','','','0','','0.00','','','0.00', 'RegistroCamionetas 1.16 Entrada: " + DateTime.Now.ToString("hh:mm:ss") + "', 'N', 'N', '' )";
                SqlCommand cmd = new SqlCommand(cadena, thisConnection);
                cmd.ExecuteNonQuery();
                thisConnection.Close();


                Android.App.AlertDialog.Builder alertDialog = new Android.App.AlertDialog.Builder(this);
                alertDialog.SetTitle(Html.FromHtml("<font color='#DF0101' size = 10>Informacion Almacenada</font>"));
                alertDialog.SetIcon(Resource.Drawable.exito);
                alertDialog.SetMessage(Html.FromHtml("<font color='#FFFFFF' size = 10>Información Grabada Correctamente!!! </font>"));
                alertDialog.SetCancelable(false);
                alertDialog.SetNeutralButton("Ok", delegate
                {
                    Guardar.Enabled = true;
                    var ampm = System.DateTime.Now.ToString("tt");
                    ampm = ampm.Replace(" ", "");
                    fechaini.Text = System.DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss ") + ampm;

                    alertDialog.Dispose();

                    //chofer.setSelection(obtenerPosicionItem(spFrutas, inicializarItem));
                    Vehiculos.SetSelection(0);
                    Chofer.SetSelection(0);
                    Destino.SetSelection(0);
                    Turno.SetSelection(0);
                    Anden.SetSelection(0);
                    Responsable.SetSelection(0);
                    TempIni.Text = "";
                    fechaIni.Text = "Fecha: 00/00/0000 00:00:00 *.M";
                });




                RunOnUiThread(() => alertDialog.Show());

            }
        }

        // Mismas consultas que antes por camioneta (pedidos con factura pendiente + pedidos split sin factura),
        // pero enviadas en lotes: un viaje al servidor por cada 100 camionetas en lugar de 2 por camioneta.
        // Requiere la conexion abierta.
        private int[] ContarPendientes(List<string> claves)
        {
            const string sqlFacturas = "SELECT Count(fcn_folio) AS cantPed  FROM  tb_mstr_facturas_nal A Where A.prov_clave = 'MRLUCKY'  AND A.cve_auto = @c{0} AND fcn_fecha > '13/02/2019' AND pdn_folio IN (SELECT pdn_folio FROM tb_mstr_pedidos_nal Where  pdn_surtido != 'S' AND pdn_estatus != 'C');";
            const string sqlSplit = "SELECT Count(emb_folio) AS pdn_folio FROM tb_det_split WHERE(estatus = 'A') AND (emb_folio IN (SELECT DISTINCT emb_folio FROM Tb_Det_Etiqueta WHERE(Cve_Camioneta = @c{0}))) AND(emb_folio NOT IN(SELECT A.pdn_folio FROM  tb_mstr_facturas_nal A Where A.prov_clave = 'MRLUCKY'  AND A.cve_auto = @c{0} AND fcn_fecha > '13/02/2019' AND pdn_folio IN (SELECT pdn_folio FROM tb_mstr_pedidos_nal Where  pdn_surtido != 'S' AND pdn_estatus != 'C')));";
            const int tamanoLote = 100;

            int[] totales = new int[claves.Count];

            for (int inicio = 0; inicio < claves.Count; inicio += tamanoLote)
            {
                int fin = Math.Min(inicio + tamanoLote, claves.Count);
                var sb = new System.Text.StringBuilder();

                using (var cmd = new SqlCommand())
                {
                    cmd.Connection = thisConnection;
                    for (int k = inicio; k < fin; k++)
                    {
                        int n = k - inicio;
                        // VarChar igual que el literal original, para conservar la comparacion y el uso de indices.
                        cmd.Parameters.Add("@c" + n, SqlDbType.VarChar, 100).Value = claves[k].Trim();
                        sb.AppendFormat(sqlFacturas, n);
                        sb.AppendFormat(sqlSplit, n);
                    }
                    cmd.CommandText = sb.ToString();

                    using (SqlDataReader rd = cmd.ExecuteReader())
                    {
                        for (int k = inicio; k < fin; k++)
                        {
                            // Dos resultados por camioneta, en el mismo orden en que se armo el lote.
                            rd.Read();
                            int valor = Convert.ToInt32(rd.GetValue(0));
                            rd.NextResult();
                            rd.Read();
                            valor += Convert.ToInt32(rd.GetValue(0));
                            rd.NextResult();
                            totales[k] = valor;
                        }
                    }
                }
            }

            return totales;
        }


    }
}

