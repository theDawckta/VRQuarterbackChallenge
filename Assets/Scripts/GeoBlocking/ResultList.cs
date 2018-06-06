using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace VOKE.VokeApp.GeoDataModel
{
	public class ResultList
	{
		public ResultList()
		{
			_listOfAddressComponents = new List<AddressComponent> ();
		}
		private List<AddressComponent> _listOfAddressComponents;

		public List<AddressComponent> ListOfAddressComponents
		{
			get{ return _listOfAddressComponents; }
			set{ _listOfAddressComponents = value; }
		}
	}
}