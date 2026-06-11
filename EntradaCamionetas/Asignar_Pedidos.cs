using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EntradaCamionetas.Modal;
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
    [Activity(Label = "AsignarPedidosySplit")]
    public class Asignar_Pedidos : Activity
    {

        SqlConnection thisConnection = new SqlConnection(MainActivity.cadenaConexion);
        //SqlDataAdapter da;
        //DataSet ds = new DataSet();
        SqlCommand cmnd = new SqlCommand();
        public static DataTable vehiculos = new DataTable("vehiculos");
        public static DataTable pedidos = new DataTable("pedidos");
        String[] strFrutas;
        String[] strPedidos;

        Spinner Vehiculos;
        Spinner Pedidos;
        EditText TempFinal;
        TextView fechaFinal;
        TextView totalcap;
        TextView totalsol;
        EditText et;

        ArrayAdapter<String> comboAdapter;
        ArrayAdapter<String> comboAdapterped;

        Button Guardar;

        public static string vehiculo = "";
        public static string horatrailer = "";
        public static string pedido = "";

        string obs = "", producto = "", observaciones = "", nombre_recibido = "", nom_reci = "";



        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            ActionBar.NavigationMode = ActionBarNavigationMode.Tabs;
            SetContentView(Resource.Layout.AsignarPedidos);

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
                // Do something when tab is selected
            };
            ActionBar.AddTab(tab, 1, true);

            tab = ActionBar.NewTab();
            tab.SetText(Resources.GetString(Resource.String.tab3_text));
            tab.SetIcon(Resource.Drawable.salir);
            tab.TabSelected += (sender, args) => {
                Intent intent2 = new Intent(this, typeof(CerrarCamioneta));
                intent2.AddFlags(ActivityFlags.ClearTop);
                intent2.AddFlags(ActivityFlags.SingleTop);
                StartActivity(intent2);
                Finish();
            };
            ActionBar.AddTab(tab, 2, false);

            Vehiculos = FindViewById<Spinner>(Resource.Id.spinner5);
            //spinner6
            Pedidos = FindViewById<Spinner>(Resource.Id.spinner6);
            TempFinal = FindViewById<EditText>(Resource.Id.TempFin);
            totalcap = FindViewById<TextView>(Resource.Id.totalcapturado);
            totalsol = FindViewById<TextView>(Resource.Id.totalsolicitado);
            fechaFinal = FindViewById<TextView>(Resource.Id.fechafin);

            
            Guardar = FindViewById<Button>(Resource.Id.Asignar);
            Guardar.Click += BtnGuardar_Click;
            Guardar.Enabled = false;


        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            observaciones = "";
            string canpedido = totalsol.Text.Replace("Detalle Split Capturado - Ped: ", "");
            canpedido = canpedido.Replace(" CJS", "");

            string cancap = totalcap.Text.Replace("Capturado: = ", "");
            cancap = cancap.Replace(" CJS", "");

            if (canpedido != cancap)
            {
                et = new EditText(this);
                AlertDialog.Builder ad = new AlertDialog.Builder(this);
                if (observaciones.Trim() != "")
                    et.Text = observaciones;

                ad.SetTitle("Ingrese Motivo de Diferencia Ped/Sur");
                ad.SetView(et);
                ad.SetPositiveButton(Html.FromHtml("<font face = 'Comic Sans MS, arial' color='#DF0101' size = '10'>Guardar</font>"), AddComentAction);
                ad.SetNegativeButton(Html.FromHtml("<font face = 'Comic Sans MS, arial' color='#DF0101' size = '10'>Cancelar</font>"), CancelComentAction);
                ad.Show();
            }
            else {
                GuardarEmbarque();
            }



        }

        private void AddComentAction(object sender, DialogClickEventArgs e)
        {
            obs = et.Text.Trim().ToUpper();
            observaciones = obs.ToUpper().Trim();

            if (observaciones.Length > 5)
            {
                GuardarEmbarque();
            }
            else {
                Toast.MakeText(this, "Debe Ingresar un Motivo de Diferencia", ToastLength.Long).Show();
            }

            
        }

        private void GuardarEmbarque()
        {

            var progressDialog = ProgressDialog.Show(this, "Espere Por Favor...", "Asignando Pedido", true);


            new System.Threading.Thread(new System.Threading.ThreadStart(delegate {//LOAD METHOD TO GET ACCOUNT INFORunOnUiThread(() => alertDialog.Show());

                thisConnection.Open();
                SqlCommand cmnd1 = new SqlCommand();

                var emb_folio = pedido.Trim();

                string cadena = "SELECT * FROM tb_det_split WHERE emb_folio = '" + pedido.Trim() + "' AND estatus = 'A'";
                SqlDataAdapter da = new SqlDataAdapter(cadena, thisConnection);
                DataSet ds = new DataSet();
                da.Fill(ds, "ConsPed");
                DataTable ConsPed = ds.Tables["ConsPed"];

                thisConnection.Close();
                foreach (DataRow Row in ConsPed.Rows)
                {


                    var tipo_rec = Row["tipo_rec"].ToString().Trim();

                    var no_lote = "";

                    if (tipo_rec == "PTC")
                    {
                        no_lote = Row["no_lote"].ToString().Trim() + Row["prod_clave"].ToString().Trim() + Convert.ToInt32(Row["TARINI"]).ToString().Trim().PadLeft(2, '0') + Convert.ToInt32(Row["TARFIN"]).ToString().Trim().PadLeft(2, '0');
                    }
                    else
                    {
                        if (Convert.ToInt32(Row["TARINI"].ToString().Trim()).ToString().Length > 1)
                        {
                            no_lote = Row["no_lote"].ToString().Trim() + Row["prod_clave"].ToString().Trim() + " " + Convert.ToInt32(Row["TARINI"].ToString().Trim());
                        }
                        else
                        {
                            no_lote = Row["no_lote"].ToString().Trim() + Row["prod_clave"].ToString().Trim() + " " + " " + Convert.ToInt32(Row["TARINI"].ToString().Trim());
                        }

                    }

                    var prod_clave = Row["prod_clave"].ToString().Trim();
                    var cajas = Row["cajas"].ToString().Trim();
                    var emb_tipo = "NAL";
                    var temp = "0";
                    var seccion = "1";
                    var recibo = Row["no_lote"].ToString().Trim();
                    int tarima = Convert.ToInt32(Row["TARINI"].ToString().Trim());
                    string fec_cad = "";
                    string id_tarima = "N/A";
                    string Estatus = "A";
                    string FechaCap = Row["FECHA"].ToString().Trim();
                    string opCap = "X";
                    int Tarima_F = Convert.ToInt32(Row["TARINI"].ToString().Trim());

                    string cadenasx = "";

                    thisConnection.Open();

                    if (tipo_rec == "PTC")
                        cadenasx = "SELECT ETIQUETA AS PROD,SURTIDO,FECHA_CAD AS FECCAD, (CASE fecha_cad WHEN '' THEN  FORMAT( DATEADD(day, 15, pti_fecha), 'dd/MM/yyyy', 'en-US' ) WHEN fecha_cad THEN fecha_cad END) AS fecha_cad FROM TB_DET_TRAZABILIDAD WHERE PROD_CLAVE = '" + prod_clave + "' AND RECIBO = '" + recibo + "' " +
                                 "AND TIPO = '" + tipo_rec + "' AND TARIMA = '" + Convert.ToInt32(tarima).ToString() + "' ";

                    else
                        cadenasx = "SELECT NUM_CAJAS AS PROD, CAJAS_SUR AS SURTIDO,NUM_LOTE AS FECCAD, ISNULL(fechacad, FORMAT( DATEADD(day, 15, fecha), 'yyyyMMdd', 'en-US' )) AS fecha_cad FROM TB_DET_ETI_FINAL WHERE CVE_PROD = '" + prod_clave + "' AND FOLIO = '" + recibo + "' " +
                            "AND TARIMA = '" + Convert.ToInt32(tarima).ToString() + "' ";

                    SqlDataAdapter dax = new SqlDataAdapter(cadenasx, thisConnection);
                    DataSet dsx = new DataSet();
                    thisConnection.Close();
                    //MessageBox.Show(cadena); 
                    dax.Fill(dsx, "Info");
                    DataTable Info = dsx.Tables["Info"];
                    //MessageBox.Show(Info.Rows.Count.ToString()); 
                    foreach (DataRow row in Info.Rows)
                    {
                        fec_cad = row["feccad"].ToString().Trim();
                    }

                    thisConnection.Open();

                    string cadenainsert = "IF NOT EXISTS(SELECT emb_folio FROM tb_det_embarque WHERE emb_folio = '" + emb_folio.Trim() + "' AND no_lote = '" + no_lote + "' AND  prod_clave = '" + prod_clave + "' AND cajas = '" + cajas + "' AND tarima = '" + tarima + "' AND tipo_rec = '" + tipo_rec + "' AND recibo = '" + recibo + "' AND FechaCap = '" + FechaCap + "' AND OpCap = '" + opCap + "') INSERT INTO tb_det_embarque(emb_folio, no_lote, prod_clave, cajas, emb_tipo, temp, seccion, tarima, fec_cad, tipo_rec, recibo, fechacad, id_tarima, Estatus, FechaCap, OpCap, Tarima_F, datecaptura) " +
                                "VALUES('" + emb_folio + "','" + no_lote + "','" + prod_clave + "','" + cajas + "','" + emb_tipo + "','" + temp + "','" + seccion + "','" + tarima + "','" + fec_cad + "','" + tipo_rec + "','" + recibo + "','','" + id_tarima + "','" +
                                Estatus + "','" + FechaCap + "','" + opCap + "','" + Tarima_F + "', GETDATE())";
                    //MessageBox.Show(cadena);
                    SqlCommand cmd = new SqlCommand(cadenainsert, thisConnection);
                    cmd.ExecuteNonQuery();

                    thisConnection.Close();


                }

                thisConnection.Open();

                //Actualizacion de Mstr_Embarque Cajas. Observaciones, Estatus, hora_Fin

                string cancap = totalcap.Text.Replace("Capturado: = ", "");
                cancap = cancap.Replace(" CJS", "");

                var ampmx = System.DateTime.Now.ToString("tt");
                ampmx = ampmx.Replace(" ", "");
                var hora = System.DateTime.Now.ToString("hh:mm ") + ampmx;


                string hora_inicio = traeinicio(emb_folio);

                string fecha_inicio = hora_inicio;


                int pos = hora_inicio.Trim().IndexOf(" ");
                //MessageBox.Show(pos.ToString()); 

                hora_inicio = hora_inicio.Substring(pos + 1, hora_inicio.Length - (pos + 1)).Trim();

                hora_inicio = hora_inicio.Replace("a. m.", "a.m.");
                hora_inicio = hora_inicio.Replace("p. m.", "p.m.");

                if (hora_inicio.Length == 12)
                {

                    hora_inicio = "0" + hora_inicio;
                }


                string hora_inicio_fin = hora_inicio.Substring(0, 5);

                hora_inicio = hora_inicio.Substring((hora_inicio.Length - 5), 5);

                hora_inicio_fin = hora_inicio_fin + hora_inicio;



                string anden = "";

                string turno = "";

                foreach (DataRow Rowi in vehiculos.Rows)
                {
                    if (vehiculo == Rowi["no_trailer"].ToString().Trim())
                    {
                        anden = Rowi["anden"].ToString().Trim();
                        turno = Rowi["turno"].ToString().Trim();
                    }

                }


                string cadenaEmb = "IF NOT EXISTS(SELECT emb_folio FROM tb_mstr_embarque WHERE emb_folio = '" + emb_folio.Trim() + "') insert into  tb_mstr_embarque(emb_tipo, emb_folio, fecha_cap, hora_ini, hora_fin, no_trailer, cajas, anden, turno, nalexp, hora_trailer, emb_obs, PESO, grabomov, equipomov, sts, guardado) " +
                                   "Values('NAL','" + emb_folio.Trim() + "','" + System.DateTime.Now.ToString("dd/MM/yyyy") + "','" + hora_inicio_fin + "','" + hora + "','" + vehiculo + "','" + cancap + "', '" + anden + "', '" + turno + "', 'NAL', '" + horatrailer + "','"+ observaciones + "','0.00','','','T','N')";
                SqlCommand cmdx = new SqlCommand(cadenaEmb, thisConnection);
                cmdx.ExecuteNonQuery();


                //Actualizacion de Det_Split Estatus**************************************

                string cadenaUpdatedetsplit = "UPDATE  tb_det_split SET estatus = 'S' Where emb_folio = '" + emb_folio + "' AND estatus != 'C'";
                //MessageBox.Show(cadena);
                SqlCommand cmdetsplit = new SqlCommand(cadenaUpdatedetsplit, thisConnection);
                cmdetsplit.ExecuteNonQuery();


                //Actualizacion de Pedidos Estatus**************************************

                string cadenaUpdatepedido = "UPDATE  tb_mstr_pedidos_nal SET pdn_surtido = 'S' Where pdn_folio = '" + emb_folio + "'";
                //MessageBox.Show(cadena);
                SqlCommand cmdped = new SqlCommand(cadenaUpdatepedido, thisConnection);
                cmdped.ExecuteNonQuery();

                thisConnection.Close();

                GuardarConsPedSur();

                Android.App.AlertDialog.Builder alertDialog = new Android.App.AlertDialog.Builder(this);
                alertDialog.SetTitle(Html.FromHtml("<font color='#DF0101' size = 10>Pedido Asignado Correctamente</font>"));
                alertDialog.SetIcon(Resource.Drawable.exito);
                alertDialog.SetMessage(Html.FromHtml("<font color='#FFFFFF' size = 10>El pedido " + emb_folio + " fue asignado a la camioneta " + vehiculo + "</font>"));
                alertDialog.SetCancelable(false);
                alertDialog.SetNeutralButton("Ok", delegate
                {
                    observaciones = "";

                    System.Collections.ArrayList listaFrutas2 = new System.Collections.ArrayList();

                    for (int i = 1; i <= pedidos.Rows.Count; i++)
                    {
                        int x = i - 1;
                        DataRow dr = pedidos.Rows[x];
                        if (emb_folio == pedidos.Rows[x]["pdn_folio"].ToString().Trim()) {
                            dr.Delete();
                            pedidos.AcceptChanges();
                            break;
                        }
                            
                    }

                    strPedidos = new String[pedidos.Rows.Count + 1];

                    if (pedidos.Rows.Count == 0)
                    {
                        strPedidos[0] = "Sin Pedidos Asignados";

                    }
                    else
                    {

                        strPedidos[0] = "Seleccione un Pedido";
                        for (int i = 1; i <= pedidos.Rows.Count; i++)
                        {
                            int x = i - 1;
                            strPedidos[i] = pedidos.Rows[x]["pdn_folio"].ToString();
                        }

                    }

                    Guardar.Enabled = false;

                    comboAdapterped = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, strPedidos);
                    Pedidos.Adapter = comboAdapterped;
                    Pedidos.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(spinner_Item_Pedidos);
                });

                RunOnUiThread(() => alertDialog.Show());
                RunOnUiThread(() => Toast.MakeText(this, "Pedido Asignado Correctamente.", ToastLength.Long).Show()); //HIDE PROGRESS DIALOG 
                RunOnUiThread(() => progressDialog.Hide());
            })).Start();
        }

        private string traeinicio(string emb_folio)
        {
            string Cadena = "Select TOP (1) fecha_Cap FROM Tb_Det_Etiqueta Where emb_folio = '" + emb_folio.Trim() + "' Order By fecha_cap ASC";
            
            SqlCommand cmd = new SqlCommand(Cadena, thisConnection);

            string TotalLeido = ""; 


            object objValue = cmd.ExecuteScalar();
            if (objValue == null)
            {
                string fecha = "SELECT SYSDATETIME()";
                SqlCommand cmdx = new SqlCommand(fecha, thisConnection);
                TotalLeido = Convert.ToDateTime(cmdx.ExecuteScalar()).ToString();
            }
            else {
                TotalLeido = cmd.ExecuteScalar().ToString();
            }

                return TotalLeido;
        }

        private void CancelComentAction(object sender, DialogClickEventArgs e)
        {
            return;
        }

        private void spinner_Item_Vehiculos(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            Spinner spinner = (Spinner)sender;
            vehiculo = spinner.GetItemAtPosition(e.Position).ToString();


            horatrailer = ValidaCamioneta(vehiculo);

            //Llenado de spinner Pedidos ***********************************************************************************************************
            thisConnection.Open();
            //string cadena = "SELECT A.pdn_folio FROM  tb_mstr_facturas_nal A INNER JOIN tb_mstr_pedidos_nal B ON A.pdn_folio = B.pdn_folio Where B.pdn_surtido != 'S' AND B.pdn_estatus != 'C' AND A.prov_clave = 'MRLUCKY'  AND A.cve_auto = '" + vehiculo.Trim() + "' AND fcn_fecha > '13/02/2019' Order by fcn_fecha DESC";
            string cadena = "SELECT A.pdn_folio FROM  tb_mstr_facturas_nal A Where A.prov_clave = 'MRLUCKY'  AND A.cve_auto = '" + vehiculo.Trim() + "' AND fcn_fecha > '13/02/2019' AND pdn_folio IN (SELECT pdn_folio FROM tb_mstr_pedidos_nal Where  pdn_surtido != 'S' AND pdn_estatus != 'C') Order by fcn_fecha DESC";
            SqlDataAdapter da = new SqlDataAdapter(cadena, thisConnection);
            DataSet ds = new DataSet();
            da.Fill(ds, "pedidos");
            pedidos = ds.Tables["pedidos"];

            thisConnection.Close();

            thisConnection.Open();
            //string cadenaoption = "SELECT DISTINCT(emb_folio) AS pdn_folio FROM tb_det_split WHERE(estatus = 'A') AND(emb_folio IN (SELECT DISTINCT emb_folio FROM Tb_Det_Etiqueta WHERE(Cve_Camioneta = '" + vehiculo.Trim() + "'))) AND(emb_folio NOT IN" +
            //"(SELECT A.pdn_folio FROM tb_mstr_facturas_nal AS A INNER JOIN tb_mstr_pedidos_nal AS B ON A.pdn_folio = B.pdn_folio WHERE(B.pdn_surtido <> 'S') AND(B.pdn_estatus <> 'C') AND(A.prov_clave = 'MRLUCKY') AND(A.cve_auto = '" + vehiculo.Trim() + "') AND(A.fcn_fecha > '13/02/2019')))";

            string cadenaoption = "SELECT DISTINCT(emb_folio) AS pdn_folio FROM tb_det_split WHERE(estatus = 'A') AND (emb_folio IN (SELECT DISTINCT emb_folio FROM Tb_Det_Etiqueta WHERE(Cve_Camioneta = '" + vehiculo.Trim() + "'))) AND(emb_folio NOT IN(SELECT A.pdn_folio FROM  tb_mstr_facturas_nal A Where A.prov_clave = 'MRLUCKY'  AND A.cve_auto = '" + vehiculo.Trim() + "' AND fcn_fecha > '13/02/2019' AND pdn_folio IN (SELECT pdn_folio FROM tb_mstr_pedidos_nal Where  pdn_surtido != 'S' AND pdn_estatus != 'C')))";

            SqlDataAdapter daoption = new SqlDataAdapter(cadenaoption, thisConnection);
            DataSet dsoption = new DataSet();
            daoption.Fill(dsoption, "pedidos");
            pedidos.Merge(dsoption.Tables["pedidos"]);

            thisConnection.Close();

            System.Collections.ArrayList listaFrutas2 = new System.Collections.ArrayList();

            strPedidos = new String[pedidos.Rows.Count + 1];

            if (pedidos .Rows.Count == 0)
            { 
                strPedidos[0] = "Sin Pedidos Asignados";

            }
            else
            {

                strPedidos[0] = "Seleccione un Pedido";
                for (int i = 1; i <= pedidos.Rows.Count; i++)
                {
                    int x = i - 1;
                    strPedidos[i] = pedidos.Rows[x]["pdn_folio"].ToString();
                }

            }

            comboAdapterped = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, strPedidos);
            Pedidos.Adapter = comboAdapterped;
            Pedidos.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(spinner_Item_Pedidos);
        }

        private void spinner_Item_Pedidos(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            Spinner spinner = (Spinner)sender;
            pedido = spinner.GetItemAtPosition(e.Position).ToString();
            
            
            List<FlimStarInfo> lstFlimStar = ConsSplit(pedido);
            var gvObject = FindViewById<GridView>(Resource.Id.gvCtrl);
            gvObject.Adapter = new myGVItemAdapter(this, lstFlimStar);
            gvObject.ItemClick += new EventHandler<AdapterView.ItemClickEventArgs>(OnGridView_ItemClicked); //detalle_pedido
            return;
        }

        protected override void OnResume()
        {
            base.OnResume(); // Always call the superclass first.
            //Llenado de spinner Responsable *******************************************************************************************************
            thisConnection.Open();
            string query = "select * FROM tb_mstr_trailer LEFT JOIN tb_cat_vehiculos ON no_trailer = clave WHERE transporte = 'PC' AND tempfin = '' AND Guardar = 'N'  AND clave != '' AND horafin = '--:--'";
            SqlDataAdapter da = new SqlDataAdapter(query, thisConnection);
            DataSet ds = new DataSet();
            da.Fill(ds, "vehiculos");
            //vehiculos.Clear();
            vehiculos = null;
            vehiculos = ds.Tables["vehiculos"];
            thisConnection.Close();

            System.Collections.ArrayList listaFrutas2 = new System.Collections.ArrayList();


            strFrutas = new String[vehiculos.Rows.Count + 1];

            if (vehiculos.Rows.Count == 0)
            {
                strFrutas[0] = "Sin vehiculos Ingresados";

            }
            else
            {
                strFrutas[0] = "Seleccione un Vehiculo";
                for (int i = 1; i <= vehiculos.Rows.Count; i++)
                {
                    int x = i - 1;
                    strFrutas[i] = vehiculos.Rows[x]["clave"].ToString();
                }
            }
            
            comboAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, strFrutas);
            Vehiculos.Adapter = comboAdapter;
            Vehiculos.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(spinner_Item_Vehiculos);

        }


        private void OnGridView_ItemClicked(object sender, AdapterView.ItemClickEventArgs e)
        {
            
        }

        private String ValidaCamioneta(string camioneta)
        {
            thisConnection.Open();
            string Cadena = "SELECT hora_trailer FROM tb_mstr_trailer WHERE horafin = '--:--' AND no_trailer = '" + camioneta.Trim() + "' AND transporte = 'PC' AND tempfin = '' AND Guardar = 'N'";
            SqlCommand cmd = new SqlCommand(Cadena, thisConnection);
            string Valor = Convert.ToString(cmd.ExecuteScalar());
            thisConnection.Close();
            return Valor;
        }

        List<FlimStarInfo> listItem = new List<FlimStarInfo>();

        List<FlimStarInfo> GetFlimStarInformation()
        {
            throw new NotImplementedException();
        }

        List<FlimStarInfo> ConsSplit(string pedido)
        {
            string Existe = "N";
            int cantidadsplit = 0;
            thisConnection.Open();

            if (pedido == "Sin Pedidos Asignados") {
                pedido = "0";
            }
            if (pedido == "Seleccione un Pedido")
            {
                pedido = "0";
            }

            string cadenatotal = "Select ISNULL(sum(pdn_num_unidades), 0) AS TotalPedido From tb_det_pedidos where pdn_folio = '" + pedido.Trim() +"'";
            SqlCommand cmd = new SqlCommand(cadenatotal, thisConnection);
            string Valor = cmd.ExecuteScalar().ToString();

            if (Valor.Replace(".000", "") == "0")
            {
                totalsol.Text = "Detalle Split Capturado";
            }
            else {
                totalsol.Text = "Detalle Split Capturado - Ped: " + Valor.Replace(".000", "") + " CJS";
            }


            listItem.Clear();
            string contenido = "";


            string cadena = "SELECT prod_clave, no_lote, cajas, nom_prod, TARINI FROM tb_det_split WHERE emb_folio = '" + pedido.Trim() + "' AND estatus = 'A'";
            SqlDataAdapter da = new SqlDataAdapter(cadena, thisConnection);
            DataSet ds = new DataSet();
            da.Fill(ds, "ConsPed");
            DataTable ConsPed = ds.Tables["ConsPed"];

            foreach (DataRow Row in ConsPed.Rows)
            {
                Existe = "S";
                listItem.Add(new FlimStarInfo()
                {
                    Name = Row["nom_prod"].ToString().Trim(),
                    Age = "Recibo: " + Row["no_lote"].ToString().Trim() + " Leido: " + Row["cajas"].ToString().Trim() + " - Tarima: " + Row["TARINI"].ToString().Trim(),
                    ImageID = Resource.Drawable.producto
                });
                cantidadsplit = cantidadsplit + Convert.ToInt32(Row["cajas"].ToString().Trim());
            }

            //LbxCons.Font = new Font(LbxCons.Font.Name, 7);   ;
            thisConnection.Close();

            totalcap.Text = "Capturado: = " + cantidadsplit + " CJS";


            //if (cantidadsplit > 0) {
                Guardar.Enabled = true;
            //}
            

            return listItem;
        }


        private void GuardarConsPedSur()
        {
            thisConnection.Open();

            string cadena = "Select * From tb_det_pedidos A, tb_Cat_producto B where a.pdn_folio = '" + pedido.Trim() + "' and a.prod_clave = b.prod_clave";
            SqlDataAdapter da = new SqlDataAdapter(cadena, thisConnection);
            DataSet ds = new DataSet();
            da.Fill(ds, "ConsPed");
            var ConsPed = ds.Tables["ConsPed"];

            foreach (DataRow Row in ConsPed.Rows)
            {
                string claveproducto = Row["prod_clave"].ToString().Trim();
                string embfolio = pedido.Trim();
                string embtipo = "NAL";
                string cant_ped = Row["pdn_num_unidades"].ToString().Trim().Replace(".000", "");
                string nom_prod = Row["prod_nombre"].ToString().Trim().Replace("'", " "); ;
                string nalexp = "NAL";
                string adicional = "";
                string borrar = "N";

                string Cadena = "Select sum(cajas) as cajas from tb_det_split Where emb_folio = '" + pedido.Trim() + "' AND prod_clave = '" + claveproducto.Trim() + "'" +
                     " AND estatus != 'C' Group By prod_clave Order by prod_clave";
                SqlCommand cmd = new SqlCommand(Cadena, thisConnection);
                int cant_sur = Convert.ToInt32(cmd.ExecuteScalar());

                string cadenaEmb = "insert into  tb_ped_embarque(emb_folio, prod_clave, emb_tipo, cant_ped, cant_sur, nom_prod, nalexp, adicional, Borrar) " +
                               "Values('" + embfolio.Trim() + "','" + claveproducto.Trim() + "','" + embtipo.Trim() + "','" + cant_ped.Trim() + "','" + cant_sur + "','" + nom_prod.Trim() + "','" + nalexp.Trim() + "', '" + adicional.Trim() + "','" + borrar.Trim() + "')";
                SqlCommand cmdx = new SqlCommand(cadenaEmb, thisConnection);

                cmdx.ExecuteNonQuery();

            }
            thisConnection.Close();

            //***********************************

        }
    }
}

