#region GPLv3

// 
// Copyright (C) 2012  Chris Chenery
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
// 

#endregion

#region Usings

using System.Collections;
using System.Collections.Generic;

#endregion

namespace IHI.Server.Libraries.Cecer1.Navigator

{
    public class Category : Listing, IEnumerator<Listing>
    {
        #region Events
        public event CategoryEventHandler OnCategoryForgot;
        public static event CategoryEventHandler OnForgotAny;

        public event ListingEventHandler OnListingAdd;
        public static event ListingEventHandler OnListingAddAny;
        public event ListingEventHandler OnListingRemove;
        public static event ListingEventHandler OnListingRemoveAny;
        #endregion

        #region Properties

        public string IdString { get; internal set; }

        public bool IsPublicCategory
        {
            get;
            internal set;
        }
        public new Category PrimaryCategory
        {
            set
            {
                base.PrimaryCategory = value;
                if (value != null)
                    IsPublicCategory = value.IsPublicCategory;
            }
            get { return base.PrimaryCategory; }
        }
        #endregion

        #region Fields

        private readonly List<Listing> _listings;

        #endregion

        #region Constructors

        public Category()
        {
            _listings = new List<Listing>();
        }

        #endregion

        #region Methods

        internal Category AddListing(Listing listing)
        {
            _listings.Add(listing);
            if (listing is Category)
                Navigator.AddCategory(listing as Category, this);

            if (OnListingAdd != null)
            OnListingAdd.Invoke(listing, new ListingEventArgs
                                             {
                                                 Navigator = Navigator
                                             });

            if (OnListingAddAny != null)
            OnListingAddAny.Invoke(listing, new ListingEventArgs
                                                {
                                                    Navigator = Navigator
                                                });

            return this;
        }

        internal Category RemoveListing(Listing listing)
        {
            if(
            _listings.Remove(listing);
            if (listing is Category)
            {
                Navigator.RemoveCategory(listing as Category);
            }

            if (OnListingRemove != null)
            OnListingRemove.Invoke(listing, new ListingEventArgs
                                                {
                                                    Navigator = Navigator
                                                });

            if (OnListingRemoveAny != null)
            OnListingRemoveAny.Invoke(listing, new ListingEventArgs
                                                   {
                                                       Navigator = Navigator
                                                   });
            return this;
        }

        internal bool ContainsListing(Listing listing)
        {
            return _listings.Contains(listing);
        }

        public ICollection<Listing> GetListings()
        {
            return _listings;
        }

        #endregion

        #region ListingEnumerator

        /// <summary>
        ///   Advances the enumerator to the next element of the collection.
        /// </summary>
        /// <returns>
        ///   true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.
        /// </returns>
        /// <exception cref = "T:System.InvalidOperationException">The collection was modified after the enumerator was created. </exception>
        /// <filterpriority>2</filterpriority>
        public bool MoveNext()
        {
            return GetListings().GetEnumerator().MoveNext();
        }

        /// <summary>
        ///   Sets the enumerator to its initial position, which is before the first element in the collection.
        /// </summary>
        /// <exception cref = "T:System.InvalidOperationException">The collection was modified after the enumerator was created. </exception>
        /// <filterpriority>2</filterpriority>
        public void Reset()
        {
            GetListings().GetEnumerator().Reset();
        }

        /// <summary>
        ///   Gets the current element in the collection.
        /// </summary>
        /// <returns>
        ///   The current element in the collection.
        /// </returns>
        /// <exception cref = "T:System.InvalidOperationException">The enumerator is positioned before the first element of the collection or after the last element.</exception>
        /// <filterpriority>2</filterpriority>
        object IEnumerator.Current
        {
            get { return Current; }
        }

        /// <summary>
        ///   Gets the element in the collection at the current position of the enumerator.
        /// </summary>
        /// <returns>
        ///   The element in the collection at the current position of the enumerator.
        /// </returns>
        public Listing Current
        {
            get { return GetListings().GetEnumerator().Current; }
        }

        /// <summary>
        ///   Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            GetListings().GetEnumerator().Dispose();
        }

        #endregion
    }
}