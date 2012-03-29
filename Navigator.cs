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

using System;
using System.Collections.Generic;
using IHI.Server.Libraries.Cecer1.Navigator;
using IHI.Server.Useful.Collections;

#endregion

namespace IHI.Server.Libraries.Cecer1.Navigator

{
    public class Navigator
    {
        #region Events
        public event ListingEventHandler OnCategoryCreated;
        #endregion

        #region Properties
        public Category GuestRoot
        {
            get;
            private set;
        }
        public Category PublicRoot
        {
            get;
            private set;
        }
        public Category NonCategory
        {
            get;
            private set;
        }
        #endregion

        #region Fields

        private readonly NestedSetDictionary<string, Category> _guestCategories;
        private readonly NestedSetDictionary<string, Category> _publicCategories;
        private readonly BiDirectionalDictionary<string, int> _numericCategoryIdCache;
        private int _nextUnusedCategoryId = 0;

        #endregion

        #region Constructors

        public Navigator()
        {
            _numericCategoryIdCache = new BiDirectionalDictionary<string, int>();

            PublicRoot = NewCategory("pLibnavRoot", "Public Rooms");
            GuestRoot = NewCategory("gLibnavRoot", "Guest Rooms");
            PublicRoot.IsPublicCategory = true;
            GuestRoot.IsPublicCategory = false;
            
            _publicCategories = new NestedSetDictionary<string, Category>("pLibnavRoot", PublicRoot);
            _guestCategories = new NestedSetDictionary<string, Category>("gLibnavRoot", GuestRoot);
            NonCategory = NewCategory("gLibnavNone", "No Category");

            CoreManager.ServerCore.GetStandardOut()
                .PrintImportant("Navigator Manager => Special categories ready:")
                .PrintImportant("                     Public = " + PublicRoot.ID)
                .PrintImportant("                     Guest = " + GuestRoot.ID)
                .PrintImportant("                     None = " + NonCategory.ID);
        }

        #endregion

        #region Methods

        #region Numerical-String ID related methods

        public Category NewCategory(string idString, string name)
        {
            Category result;
            if (_publicCategories != null && _publicCategories.TryGetValue(idString, out result))
                return result;
            if (_guestCategories != null && _guestCategories.TryGetValue(idString, out result))
                return result;

            int numericalID = _nextUnusedCategoryId++;
            _numericCategoryIdCache.Add(idString, numericalID);

            Category category = new Category
                                    {
                                        ID = numericalID,
                                        IdString = idString,
                                        Name = name,
                                        Navigator = this
                                    };

            CoreManager.ServerCore.GetStandardOut().PrintDebug("Navigator Manager => Category created: " + numericalID +
                                                               ", idstring, " + name);

            if(OnCategoryCreated != null)
                OnCategoryCreated.Invoke(category, new CategoryEventArgs
                                                   {
                                                       Navigator = this
                                                   });
            return category;
        }

        public Navigator ForgetCategory(Category category)
        {
            if (_publicCategories.ContainsKey(category.IdString))
                throw new NavigatorException("Unable to forget category \"" +
                                             category.IdString + "\" because the category exists in the public tree.");

            if (_guestCategories.ContainsKey(category.IdString))
                throw new NavigatorException("Unable to forget category \"" +
                                             category.IdString + "\" because the category exists in the guest tree.");

            _numericCategoryIdCache.TryRemoveByFirst(category.IdString);
            return this;
        }
        public Category GetCategory(int numericalID)
        {
            string idString;
            if(_numericCategoryIdCache.TryGetBySecond(numericalID, out idString))
            {
                return GetCategory(idString);
            }
            return null;
        }

        #endregion

        #region Category Tree related methods

        public Category GetCategory(string idString,
                                    NavigatorTreeSearchMode treeSearchMode = NavigatorTreeSearchMode.PublicFirst)
        {
            Category result = null;
            switch (treeSearchMode)
            {
                case NavigatorTreeSearchMode.PublicOnly:
                    {
                        _publicCategories.TryGetValue(idString, out result);
                        break;
                    }
                case NavigatorTreeSearchMode.GuestOnly:
                    {
                        _guestCategories.TryGetValue(idString, out result);
                        break;
                    }
                case NavigatorTreeSearchMode.PublicFirst:
                    {
                        _publicCategories.TryGetValue(idString, out result);
                        if (result == null)
                            _guestCategories.TryGetValue(idString, out result);
                        break;
                    }
                case NavigatorTreeSearchMode.GuestFirst:
                    {
                        _guestCategories.TryGetValue(idString, out result);
                        if (result == null)
                            _publicCategories.TryGetValue(idString, out result);
                        break;
                    }
            }

            return result;
        }

        public ICollection<Category> GetChildren(Category category,
                                                 NavigatorTreeSearchMode treeSearchMode =
                                                     NavigatorTreeSearchMode.PublicFirst)
        {
            switch (treeSearchMode)
            {
                case NavigatorTreeSearchMode.PublicOnly:
                    {
                        if (!_publicCategories.ContainsKey(category.IdString))
                            throw new NavigatorException("This instance of Navigator does not contain a \"" +
                                                         category.IdString + "\" category. SearchMode: PublicOnly");
                        return _publicCategories.GetChildren(category.IdString);
                    }
                case NavigatorTreeSearchMode.GuestOnly:
                    {
                        if (!_guestCategories.ContainsKey(category.IdString))
                            throw new NavigatorException("This instance of Navigator does not contain a \"" +
                                                         category.IdString + "\" category. SearchMode: GuestOnly");
                        return _guestCategories.GetChildren(category.IdString);
                    }
                case NavigatorTreeSearchMode.PublicFirst:
                    {
                        if (!_publicCategories.ContainsKey(category.IdString))
                            if (!_guestCategories.ContainsKey(category.IdString))
                                throw new NavigatorException("This instance of Navigator does not contain a \"" +
                                                             category.IdString + "\" category. SearchMode: PublicFirst");
                            else
                                return _guestCategories.GetChildren(category.IdString);
                            return _publicCategories.GetChildren(category.IdString);
                    }
                case NavigatorTreeSearchMode.GuestFirst:
                    {
                        if (!_guestCategories.ContainsKey(category.IdString))
                            if (!_publicCategories.ContainsKey(category.IdString))
                                throw new NavigatorException("This instance of Navigator does not contain a \"" +
                                                             category.IdString + "\" category. SearchMode: GuestFirst");
                            else
                                return _publicCategories.GetChildren(category.IdString);
                            return _guestCategories.GetChildren(category.IdString);
                    }
                default:
                    {
                        throw new NavigatorException("Invalid SearchMode.");
                    }
            }
        }

        internal Navigator AddCategory(Category category, Category parent,
                                     NavigatorTreeSearchMode treeSearchMode = NavigatorTreeSearchMode.PublicFirst)
        {
            switch (treeSearchMode)
            {
                case NavigatorTreeSearchMode.PublicOnly:
                    {
                        if (!_publicCategories.ContainsKey(parent.IdString))
                            throw new NavigatorException("This instance of Navigator does not contain a \"" +
                                                         parent.IdString + "\" category. SearchMode: PublicOnly");
                        _publicCategories.AddAsChildOf(category.IdString, category, parent.IdString);
                        break;
                    }
                case NavigatorTreeSearchMode.GuestOnly:
                    {
                        if (!_guestCategories.ContainsKey(parent.IdString))
                            throw new NavigatorException("This instance of Navigator does not contain a \"" +
                                                         parent.IdString + "\" category. SearchMode: GuestOnly");
                        _guestCategories.AddAsChildOf(category.IdString, category, parent.IdString);
                        break;
                    }
                case NavigatorTreeSearchMode.PublicFirst:
                    {
                        if (!_publicCategories.ContainsKey(parent.IdString))
                            if (!_guestCategories.ContainsKey(parent.IdString))
                                throw new NavigatorException("This instance of Navigator does not contain a \"" +
                                                             parent.IdString + "\" category. SearchMode: PublicFirst");
                            else
                                _guestCategories.AddAsChildOf(category.IdString, category, parent.IdString);
                        else
                            _publicCategories.AddAsChildOf(category.IdString, category, parent.IdString);
                        break;
                    }
                case NavigatorTreeSearchMode.GuestFirst:
                    {
                        if (!_guestCategories.ContainsKey(parent.IdString))
                            if (!_publicCategories.ContainsKey(parent.IdString))
                                throw new NavigatorException("This instance of Navigator does not contain a \"" +
                                                             parent.IdString + "\" category. SearchMode: GuestFirst");
                            else
                                _publicCategories.AddAsChildOf(category.IdString, category, parent.IdString);
                        else
                            _guestCategories.AddAsChildOf(category.IdString, category, parent.IdString);
                        break;
                    }
            }
            CoreManager.ServerCore.GetStandardOut().PrintDebug("Navigator Manager => Category " + category.IdString + " adopted by " + parent.IdString);
            return this;
        }

        internal Navigator RemoveCategory(Category category, NavigatorTreeSearchMode treeSearchMode = NavigatorTreeSearchMode.PublicFirst, NestedSetRemoveChildAction childAction = NestedSetRemoveChildAction.MoveUpGeneration)
        {
            switch (treeSearchMode)
            {
                case NavigatorTreeSearchMode.PublicOnly:
                    {
                        if (!_publicCategories.ContainsKey(category.IdString))
                            throw new NavigatorException("This instance of Navigator does not contain a \"" +
                                                         category.IdString + "\" category. SearchMode: PublicOnly");
                        _publicCategories.Remove(category.IdString, childAction);
                        break;
                    }
                case NavigatorTreeSearchMode.GuestOnly:
                    {
                        if (!_guestCategories.ContainsKey(category.IdString))
                            throw new NavigatorException("This instance of Navigator does not contain a \"" +
                                                         category.IdString + "\" category. SearchMode: GuestOnly");
                        _guestCategories.Remove(category.IdString, childAction);
                        break;
                    }
                case NavigatorTreeSearchMode.PublicFirst:
                    {
                        if (!_publicCategories.ContainsKey(category.IdString))
                            if (!_guestCategories.ContainsKey(category.IdString))
                                throw new NavigatorException("This instance of Navigator does not contain a \"" +
                                                             category.IdString + "\" category. SearchMode: PublicFirst");
                            else
                                _guestCategories.Remove(category.IdString, childAction);
                        else
                            _publicCategories.Remove(category.IdString, childAction);
                        break;
                    }
                case NavigatorTreeSearchMode.GuestFirst:
                    {
                        if (!_guestCategories.ContainsKey(category.IdString))
                            if (!_publicCategories.ContainsKey(category.IdString))
                                throw new NavigatorException("This instance of Navigator does not contain a \"" +
                                                             category.IdString + "\" category. SearchMode: GuestFirst");
                            else
                                _publicCategories.Remove(category.IdString, childAction);
                        else
                            _guestCategories.Remove(category.IdString, childAction);
                        break;
                    }
            }
            return this;
        }

        #endregion
        #endregion
    }
}