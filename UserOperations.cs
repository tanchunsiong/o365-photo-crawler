// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license. See full license at the bottom of this file.
using Microsoft.Graph;
using Microsoft.OData.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;


namespace O365_Win_Profile
{
    class UserOperations
    {

        /// <summary>
        /// Gets a list of users on the current tenant.
        /// </summary>
        /// <returns>List<IUser> </returns>
        public async Task<List<IUser>> GetUsersAsync()
        {
            try
            {
                List<IUser> userList = null;

                var graphClient = await AuthenticationHelper.GetGraphClientAsync();

                var userResult = await graphClient.users.Where( u=> u.userType == "Member").ExecuteAsync();
                userList = userResult.CurrentPage.ToList();

                return userList;
            }

            catch (DataServiceQueryException dsqe)
            {
                Debug.WriteLine("Could not get users: " + dsqe.InnerException.Message);
                return null;
            }

            catch (Exception e)
            {
                Debug.WriteLine("Could not get users: " + e.Message);
                return null;
            }
        }

        /// <summary>
        /// Gets the manager of the specified user.
        /// </summary>
        /// <returns>User </returns>
        public async Task<User> GetUserManagerAsync(string userId)
        {
            try
            {
                User manager = null;

                var graphClient = await AuthenticationHelper.GetGraphClientAsync();

                var managerResult = await graphClient.users.GetById(userId).manager.ExecuteAsync();
                manager = (User)managerResult;

                return manager;
            }

            catch (DataServiceQueryException dsqe)
            {
                Debug.WriteLine("Could not get manager: " + dsqe.InnerException.Message);
                return null;
            }

            catch (Exception e)
            {
                Debug.WriteLine("Could not get manager: " + e.Message);
                return null;
            }

        }

        /// <summary>
        /// Gets the specified user.
        /// </summary>
        /// <returns>User </returns>

        public async Task<User> GetUserAsync(string userId)
        {
            try
            {
                User user = null;

                var graphClient = await AuthenticationHelper.GetGraphClientAsync();

                var userResult = await graphClient.users.GetById(userId).ExecuteAsync();
                user = (User)userResult;

                return user;
            }

            catch (DataServiceQueryException dsqe)
            {
                Debug.WriteLine("Could not get user: " + dsqe.InnerException.Message);
                return null;
            }

            catch (Exception e)
            {
                Debug.WriteLine("Could not get user: " + e.Message);
                return null;
            }

        }

        /// <summary>
        /// Gets the specified user's direct reports.
        /// </summary>
        /// <returns>List<IDirectoryObject> </returns>
        public async Task<List<IDirectoryObject>> GetUserDirectReportsAsync(string userId)
        {
            try
            {

                var graphClient = await AuthenticationHelper.GetGraphClientAsync();

                var directReportResult = await graphClient.users.GetById(userId).directReports.ExecuteAsync();
                var directReportList = directReportResult.CurrentPage.ToList();

                return directReportList;
            }

            catch (DataServiceQueryException dsqe)
            {
                Debug.WriteLine("Could not get direct reports: " + dsqe.InnerException.Message);
                return null;
            }

            catch (Exception e)
            {
                Debug.WriteLine("Could not get direct reports: " + e.Message);
                return null;
            }

        }

        /// <summary>
        /// Gets groups to which the specified user belongs. 
        /// </summary>
        /// <returns><List<IDirectoryObject> </returns>
        public async Task<List<IDirectoryObject>> GetUserGroupsAsync(string userId)
        {
            try 
            {
                var graphClient = await AuthenticationHelper.GetGraphClientAsync();
                var groupResult = await graphClient.users.GetById(userId).memberOf.ExecuteAsync();
                var groupList = groupResult.CurrentPage.ToList();

                return groupList;
            }

            catch (DataServiceQueryException dsqe)
            {
                Debug.WriteLine("Could not get groups: " + dsqe.InnerException.Message);
                return null;
            }

            catch (Exception e)
            {

                Debug.WriteLine("Could not get groups: " + e.Message);
                return null;
            }

        }

        /// <summary>
        /// Gets files that are shared with the user.
        /// </summary>
        /// <returns>List<IItem> </returns>
        public async Task<List<IItem>> GetUserFilesAsync(string userId)
        {
            try
            {
                var graphClient = await AuthenticationHelper.GetGraphClientAsync();

                var filesResult = await graphClient.users.GetById(userId).files.Take(10).ExecuteAsync();
                var fileList = filesResult.CurrentPage.ToList();

                return fileList;
            }

            catch (DataServiceQueryException dsqe)
            {
                Debug.WriteLine("Could not get files: " + dsqe.InnerException.Message);
                return null;
            }

            catch (Exception e)
            {

                Debug.WriteLine("Could not get files: " + e.Message);
                return null;
            }


        }

        /// <summary>
        /// Gets the user's thumbnail photo.
        /// </summary>
        /// <returns>BitmapImage </returns>

        // Using a REST request for photo after getting the URI for the thumbnail stream
        public async Task<BitmapImage> GetPhotoAsync(string photoUrl, string token, string alias)
        {

            using (var client = new HttpClient())
            {
                try
                {
                    var request = new HttpRequestMessage(HttpMethod.Get, new Uri(photoUrl));
                    BitmapImage bitmap = null;

                    request.Headers.Add("Authorization", "Bearer " + token);

                    var response = await client.SendAsync(request);

                    var stream = await response.Content.ReadAsStreamAsync();
                    if (response.IsSuccessStatusCode)
                    {

                        
                            //hijack Image here
                            
                            var jpgProperties = ImageEncodingProperties.CreateJpeg();
                            jpgProperties.Width = 800;
                            jpgProperties.Height = 600;
                            var properties = jpgProperties.Properties;

                            Guid photoID = System.Guid.NewGuid();
                            String photolocation = photoID.ToString() + ".jpg";  //file name

                        using (var irandomstream = stream.AsRandomAccessStream())
                        {

                            //create decoder and transform
                            BitmapDecoder dec = await BitmapDecoder.CreateAsync(irandomstream);
                            BitmapTransform transform = new BitmapTransform();

                            //roate the image
                            transform.Rotation = BitmapRotation.None;
                            //transform.Bounds = GetFifteenByNineBounds();
                            transform.Bounds = new BitmapBounds() { Height = dec.PixelHeight, Width = dec.PixelWidth, X = 0, Y = 0 };
                            //get the conversion data that we need to save the cropped and rotated image
                            BitmapPixelFormat pixelFormat = dec.BitmapPixelFormat;
                            BitmapAlphaMode alpha = dec.BitmapAlphaMode;

                            //read the PixelData
                            PixelDataProvider pixelProvider = await dec.GetPixelDataAsync(
                                pixelFormat,
                                alpha,
                                transform,
                                ExifOrientationMode.RespectExifOrientation,
                                ColorManagementMode.ColorManageToSRgb
                                );
                            byte[] pixels = pixelProvider.DetachPixelData();


                            var folder = Windows.Storage.ApplicationData.Current.LocalFolder;

                            StorageFolder x =await folder.CreateFolderAsync(alias,CreationCollisionOption.OpenIfExists);
                            StorageFile file = await x.CreateFileAsync(photolocation);

                            irandomstream.Seek(0);
                            bitmap = new BitmapImage();

                            await bitmap.SetSourceAsync(irandomstream);

                            //writing directly into the file stream
                            using (IRandomAccessStream convertedImageStream = await file.OpenAsync(FileAccessMode.ReadWrite))
                            {
                                //write changes to the BitmapEncoder
                                BitmapEncoder enc = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, convertedImageStream);
                                enc.SetPixelData(
                                    pixelFormat,
                                    alpha,
                                    transform.Bounds.Width,
                                    transform.Bounds.Height,
                                    dec.DpiX,
                                    dec.DpiY,
                                    pixels
                                    );

                                await enc.FlushAsync();



                            }



                        }


                            

                        //await stream.CopyToAsync(memStream);
                        //memStream.Seek(0, SeekOrigin.Begin);
                        return bitmap;
                    }
                           
                        
                    

                    else
                    {
                        Debug.WriteLine("Unable to find an image at this endpoint.");
                        bitmap = new BitmapImage(new Uri("ms-appx:///assets/UserDefault.png", UriKind.RelativeOrAbsolute));
                        return bitmap;
                    }

                }

                catch (Exception e)
                {
                    Debug.WriteLine("Could not get the thumbnail photo: " + e.Message);
                    return null;
                }
            }

        }

    }
}

//********************************************************* 
// 
//O365-Win-Profile, https://github.com/OfficeDev/O365-Win-Profile
//
//Copyright (c) Microsoft Corporation
//All rights reserved. 
//
// MIT License:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// ""Software""), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:

// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 
//********************************************************* 