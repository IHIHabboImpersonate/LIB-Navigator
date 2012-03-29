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

using System.Collections.Generic;

#endregion

namespace IHI.Server.Libraries.Cecer1.Navigator

{
    public abstract class RoomListing : Listing
    {
        #region Fields

        private ICollection<Category> _secondaryCategories;

        #endregion

        #region Properties
        
        /// <summary>
        ///   The description of the room.
        /// </summary>
        public string Description { get; set; }

        #endregion

        #region Methods

        public RoomListing AddSecondaryCategory(Category category)
        {
            if (_secondaryCategories.Contains(category))
                return this;
            _secondaryCategories.Add(category);
            category.AddListing(this);
            return this;
        }

        public RoomListing RemoveSecondaryCategory(Category category)
        {
            if (!_secondaryCategories.Contains(category))
                return this;
            category.RemoveListing(this);
            _secondaryCategories.Remove(category);
            return this;
        }

        #endregion
    }
}