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
using EntradaCamionetas.Modal;

namespace EntradaCamionetas
{
    public class myGVItemAdapter : BaseAdapter<EntradaCamionetas.Modal.FlimStarInfo>
    {
        Activity _CurrentContext;
        List<EntradaCamionetas.Modal.FlimStarInfo> _lstFlimStarInfo;

        public myGVItemAdapter(Activity currentContext, List<EntradaCamionetas.Modal.FlimStarInfo> lstFlimInfo)
        {
            _CurrentContext = currentContext;
            _lstFlimStarInfo = lstFlimInfo;
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var item = _lstFlimStarInfo[position];
            if (convertView == null)
                convertView = _CurrentContext.LayoutInflater.Inflate(Resource.Layout.custGridViewItem, null);

            convertView.FindViewById<TextView>(Resource.Id.txtName).Text = item.Name;
            convertView.FindViewById<TextView>(Resource.Id.txtAge).Text = item.Age.ToString();
            convertView.FindViewById<ImageView>(Resource.Id.imgPers).SetImageResource(item.ImageID);

            return convertView;
        }

        public override int Count
        {
            get { return _lstFlimStarInfo == null ? -1 : _lstFlimStarInfo.Count; }
        }

        public override EntradaCamionetas.Modal.FlimStarInfo this[int position] => _lstFlimStarInfo == null ? null : _lstFlimStarInfo[position];

    }
}
