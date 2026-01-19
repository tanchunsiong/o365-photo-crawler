using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;


using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Shapes;
using ClientContract = Microsoft.ProjectOxford.Face.Contract;
using Windows.UI.Xaml.Navigation;
using Microsoft.IdentityModel.Clients.ActiveDirectory;


// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace O365_Win_Profile
{




    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SimplePage : Page, INotifyPropertyChanged
    {
        private string _mailAddress;
        private string _displayName = null;
        private MailHelper _mailHelper = new MailHelper();
        private bool _userLoggedIn = false;
        public static ApplicationDataContainer _settings = ApplicationData.Current.LocalSettings;


        #region Fields

        /// <summary>
        /// Description dependency property
        /// </summary>
        public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register("Description", typeof(string), typeof(MainPage), new PropertyMetadata("Hello world"));

        /// <summary>
        /// Output dependency property
        /// </summary>
        public static readonly DependencyProperty OutputProperty = DependencyProperty.Register("Output", typeof(string), typeof(MainPage), new PropertyMetadata("Hello world"));

        /// <summary>
        /// Temporary group name for create person database
        /// </summary>
        ///
        //dreamtcs to fix this
        //public static readonly string SampleGroupName = "raspberr-ypi2-403d-b7fd-chunsiongtan";
        public static readonly string SampleGroupName = "raspberr-ypi2-403d-b7fd-chunsiongtan";

        /// <summary>
        /// Faces to identify
        /// </summary>
        private ObservableCollection<Face> _faces = new ObservableCollection<Face>();

        /// <summary>
        /// Person database
        /// </summary>
        private ObservableCollection<Person> _persons = new ObservableCollection<Person>();

        /// <summary>
        /// User picked image file path
        /// </summary>
        private string _selectedFile;
        //dreamtcs
        private bool appendMode = false;
        #endregion Fields

        #region Events

        /// <summary>
        /// Implement INotifyPropertyChanged interface
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Events

        /// <summary>
        /// Identification result for UI binding
        /// </summary>
        public class IdentificationResult : INotifyPropertyChanged
        {
            #region Fields

            /// <summary>
            /// Face to identify
            /// </summary>
            private Face _faceToIdentify;

            /// <summary>
            /// Identified person's name
            /// </summary>
            private string _name;

            #endregion Fields

            #region Events

            /// <summary>
            /// Implement INotifyPropertyChanged interface
            /// </summary>
            public event PropertyChangedEventHandler PropertyChanged;

            #endregion Events

            #region Properties

            /// <summary>
            /// Gets or sets face to identify
            /// </summary>
            public Face FaceToIdentify
            {
                get
                {
                    return _faceToIdentify;
                }

                set
                {
                    _faceToIdentify = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("FaceToIdentify"));
                    }
                }
            }

            /// <summary>
            /// Gets or sets identified person's name
            /// </summary>
            public string Name
            {
                get
                {
                    return _name;
                }

                set
                {
                    _name = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("Name"));
                    }
                }
            }

            #endregion Properties
        }

        /// <summary>
        /// Person structure for UI binding
        /// </summary>
        public class Person : INotifyPropertyChanged
        {
            #region Fields

            /// <summary>
            /// Person's faces from database
            /// </summary>
            private ObservableCollection<Face> _faces = new ObservableCollection<Face>();

            /// <summary>
            /// Person's id
            /// </summary>
            private string _personId;

            /// <summary>
            /// Person's name
            /// </summary>
            private string _personName;

            #endregion Fields

            #region Events

            /// <summary>
            /// Implement INotifyPropertyChanged interface
            /// </summary>
            public event PropertyChangedEventHandler PropertyChanged;

            #endregion Events

            #region Properties

            /// <summary>
            /// Gets or sets person's faces from database
            /// </summary>
            public ObservableCollection<Face> Faces
            {
                get
                {
                    return _faces;
                }

                set
                {
                    _faces = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("Faces"));
                    }
                }
            }

            /// <summary>
            /// Gets or sets person's id
            /// </summary>
            /// 
            public string PersonId
            {
                get
                {
                    return _personId;
                }

                set
                {
                    _personId = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("PersonId"));
                    }
                }
            }

            /// <summary>
            /// Gets or sets person's name
            /// </summary>
            public string PersonName
            {
                get
                {
                    return _personName;
                }

                set
                {
                    _personName = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("PersonName"));
                    }
                }
            }

            #endregion Properties
        }

        #region Properties

        /// <summary>
        /// Gets or sets description
        /// </summary>
        public string Description
        {
            get
            {
                return (string)GetValue(DescriptionProperty);
            }

            set
            {
                SetValue(DescriptionProperty, value);
                OnPropertyChanged<string>();
            }
        }

        /// <summary>
        /// Helper function for INotifyPropertyChanged interface 
        /// </summary>
        /// <typeparam name="T">Property type</typeparam>
        /// <param name="caller">Property name</param>
        private void OnPropertyChanged<T>([CallerMemberName]string caller = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(caller));
            }
        }

        /// <summary>
        /// Gets group name
        /// </summary>
        public string GroupName
        {
            get
            {
                return SampleGroupName;
            }
        }

        /// <summary>
        /// Gets constant maximum image size for rendering detection result
        /// </summary>
        public int MaxImageSize
        {
            get
            {
                return 300;
            }
        }

        /// <summary>
        /// Gets or sets output
        /// </summary>
        public string Output
        {
            get
            {
                return (string)GetValue(OutputProperty);
            }

            set
            {
                SetValue(OutputProperty, value);
                OnPropertyChanged<string>();
            }
        }

        /// <summary>
        /// Gets person database
        /// </summary>
        public ObservableCollection<Person> Persons
        {
            get
            {
                return _persons;
            }
        }

        /// <summary>
        /// Gets or sets user picked image file path
        /// </summary>
        public string SelectedFile
        {
            get
            {
                return _selectedFile;
            }

            set
            {
                _selectedFile = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("SelectedFile"));
                }
            }
        }

        /// <summary>
        /// Gets faces to identify
        /// </summary>
        public ObservableCollection<Face> TargetFaces
        {
            get
            {
                return _faces;
            }
        }

        #endregion Properties



        // TODO: Move subscription key to app configuration
        String SubscriptionKey = "YOUR_AZURE_FACE_API_SUBSCRIPTION_KEY";
        public SimplePage()
        {
            this.InitializeComponent();
            this.DataContext = this;
            App.Initialize(this.SubscriptionKey);
            LoadItems();

        }

        private async void LoadItems()
        {
            bool groupExists = false;

            // Test whether the group already exists
            try
            {
                Output = Output.AppendLine(string.Format("Request: Group {0} will be used for build person database. Checking whether group exists.", GroupName));
                await App.Instance.GetPersonGroupAsync(GroupName);
                groupExists = true;
                Output = Output.AppendLine(string.Format("Response: Group {0} exists.", GroupName));
            }
            catch (ClientException ex)
            {
                if (ex.Error.Code != "PersonGroupNotFound")
                {
                    Output = Output.AppendLine(string.Format("Response: {0}. {1}", ex.Error.Code, ex.Error.Message));
                    return;
                }
                else
                {
                    Output = Output.AppendLine(string.Format("Response: Group {0} does not exist before.", GroupName));
                }
            }
            //dreamtcs append mode
            if (!appendMode)
            {
                if (groupExists)
                {
                    await App.Instance.DeletePersonGroupAsync(GroupName);
                }
            }
            // Show folder picker


            var folder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            folder = await folder.GetFolderAsync("Assets\\PersonGroup");


            // Set the suggestion count is intent to minimum the data preparetion step only,
            // it's not corresponding to service side constraint
            const int SuggestionCount = 20000;

            if (folder != null)
            {
                // User picked a root person database folder
                // Clear person database
                Persons.Clear();
                TargetFaces.Clear();
                SelectedFile = null;

                // Call create person group REST API
                // Create person group API call will failed if group with the same name already exists

                Output = Output.AppendLine(string.Format("Request: Creating group \"{0}\"", GroupName));
                if (!appendMode)
                {
                    try
                    {
                        await App.Instance.CreatePersonGroupAsync(GroupName, GroupName);
                        Output = Output.AppendLine(string.Format("Response: Success. Group \"{0}\" created", GroupName));
                    }
                    catch (ClientException ex)
                    {
                        Output = Output.AppendLine(string.Format("Response: {0}. {1}", ex.Error.Code, ex.Error.Message));
                        return;
                    }
                }
                int processCount = 0;
                bool forceContinue = false;

                Output = Output.AppendLine("Request: Preparing faces for identification, detecting faces in choosen folder.");

                // Enumerate top level directories, each directory contains one person's images
                foreach (var dir in await folder.GetFoldersAsync())
                {
                    var tasks = new List<Task>();
                    var tag = dir.Name;
                    Person p = new Person();
                    p.PersonName = tag;

                    // Call create person REST API, the new create person id will be returned
                    var faces = new ObservableCollection<Face>();
                    p.Faces = faces;
                    Persons.Add(p);

                    // Enumerate images under the person folder, call detection
                    foreach (var img in await dir.GetFilesAsync())
                    {
                        if (img.FileType.Equals(".jpg") || img.FileType.Equals(".JPG"))
                        {
                            tasks.Add(Task.Factory.StartNew(
                                async (obj) =>
                                {
                                    var imgPath = obj as StorageFile;

                                    // Call detection REST API
                                   
                                    using (var fStream = await imgPath.OpenStreamForReadAsync())
                                    {
                                        try
                                        {
                                            var face = await App.Instance.DetectAsync(fStream);
                                            if (appendMode)
                                            {
                                                //Serialize(face, imgPath + ".face");
                                            }
                                            return new Tuple<string, ClientContract.Face[]>(imgPath.Path, face);
                                        }
                                        catch (ClientException)
                                        {
                                            // Here we simply ignore all detection failure in this sample
                                            // You may handle these exceptions by check the Error.Code and Error.Message property for ClientException object
                                            return new Tuple<string, ClientContract.Face[]>(imgPath.Path, null);
                                        }
                                    }
                                },
                                img).Unwrap().ContinueWith((detectTask) =>
                                {
                                    // Update detected faces for rendering
                                    var detectionResult = detectTask.Result;
                                    if (detectionResult == null || detectionResult.Item2 == null)
                                    {
                                        return;
                                    }

                                    foreach (var f in detectionResult.Item2)
                                    {
                                        //this.Dispatcher.Invoke(
                                        //    new Action<ObservableCollection<Face>, string, ClientContract.Face>(UIHelper.UpdateFace),
                                        //    faces,
                                        //    detectionResult.Item1,
                                        //    f);
                                        CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                                           () =>
                                           {
                                               UIHelper.UpdateFace(faces,
                                                                       detectionResult.Item1,
                                                                       f);
                                           });

                                    }
                                }));
                            if (processCount >= SuggestionCount && !forceContinue)
                            {
                                //var continueProcess = System.Windows.Forms.MessageBox.Show("The images loaded have reached the recommended count, may take long time if proceed. Would you like to continue to load images?", "Warning", MessageBoxButtons.YesNo);
                                //if (continueProcess == DialogResult.Yes)
                                //{
                                //    forceContinue = true;
                                //}
                                //else
                                //{
                                //    break;
                                //}
                            }
                        }

                        await Task.WhenAll(tasks);
                    }
                }

                Output = Output.AppendLine(string.Format("Response: Success. Total {0} faces are detected.", Persons.Sum(p => p.Faces.Count)));

                try
                {
                    // Update person faces on server side
                    foreach (var p in Persons)
                    {
                        // Call person update REST API
                        Output = Output.AppendLine(string.Format("Request: Creating person \"{0}\"", p.PersonName));
                        p.PersonId = (await App.Instance.CreatePersonAsync(GroupName, p.Faces.Select(face => Guid.Parse(face.FaceId)).ToArray(), p.PersonName)).PersonId.ToString();

                        Output = Output.AppendLine(string.Format("Response: Success. Person \"{0}\" (PersonID:{1}) created, {2} face(s) added.", p.PersonName, p.PersonId, p.Faces.Count));
                    }

                    // Start train person group
                    Output = Output.AppendLine(string.Format("Request: Training group \"{0}\"", GroupName));
                    if (Persons.Count >= 1)
                    {
                        await App.Instance.TrainPersonGroupAsync(GroupName);

                        // Wait until train completed
                        while (true)
                        {
                            await Task.Delay(1000);
                            var status = await App.Instance.GetPersonGroupTrainingStatusAsync(GroupName);
                            Output = Output.AppendLine(string.Format("Response: {0}. Group \"{1}\" training process is {2}", "Success", GroupName, status.Status));
                            if (status.Status != "running")
                            {
                                break;
                            }
                        }
                    }
                }
                catch (ClientException ex)
                {
                    Output = Output.AppendLine(string.Format("Response: {0}. {1}", ex.Error.Code, ex.Error.Message));
                }
                folder = Windows.ApplicationModel.Package.Current.InstalledLocation;
                folder = await folder.GetFolderAsync("Assets\\PersonGroup-Trained");
                //dreamtcs reload all trained
                foreach (var dir in await folder.GetFoldersAsync())
                {
                    var tasks = new List<Task>();
                    var tag = dir.Name;
                    Person p = new Person();
                    p.PersonName = tag;

                    // Call create person REST API, the new create person id will be returned
                    var faces = new ObservableCollection<Face>();
                    p.Faces = faces;
                    Persons.Add(p);

                    // Enumerate images under the person folder, call detection
                    foreach (var img in await dir.GetFilesAsync())
                    {
                        if (img.FileType.Equals(".jpg"))
                        {

                            tasks.Add(Task.Factory.StartNew(
                                async (obj) =>
                                {
                                    var imgPath = obj as StorageFile;

                                    // Call detection REST API
                                  
                                  
                                    using (var fStream = await imgPath.OpenStreamForReadAsync())
                                    {
                                        try
                                        {
                                            //dreamtcs

                                            var face = await App.Instance.DetectAsync(fStream);

                                            return new Tuple<string, ClientContract.Face[]>(imgPath.Path, face);

                                        }
                                        catch (ClientException)
                                        {
                                            // Here we simply ignore all detection failure in this sample
                                            // You may handle these exceptions by check the Error.Code and Error.Message property for ClientException object
                                            return new Tuple<string, ClientContract.Face[]>(imgPath.Path, null);
                                        }
                                    }
                                },
                                img).Unwrap().ContinueWith((detectTask) =>
                                {
                                    // Update detected faces for rendering
                                    var detectionResult = detectTask.Result;
                                    if (detectionResult == null || detectionResult.Item2 == null)
                                    {
                                        return;
                                    }

                                    foreach (var f in detectionResult.Item2)
                                    {
                                        //this.Dispatcher.Invoke(
                                        //    new Action<ObservableCollection<Face>, string, ClientContract.Face>(UIHelper.UpdateFace),
                                        //    faces,
                                        //    detectionResult.Item1,
                                        //    f);

                                        Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                                            () =>
                                            {
                                                UIHelper.UpdateFace(faces,
                                                                    detectionResult.Item1,
                                                                    f);
                                            });
                                    }
                                }));
                            if (processCount >= SuggestionCount && !forceContinue)
                            {
                                //var continueProcess = System.Windows.Forms.MessageBox.Show("The images loaded have reached the recommended count, may take long time if proceed. Would you like to continue to load images?", "Warning", MessageBoxButtons.YesNo);
                                //if (continueProcess == DialogResult.Yes)
                                //{
                                //    forceContinue = true;
                                //}
                                //else
                                //{
                                //    break;
                                //}
                            }
                        }
                    }
                    try
                    {
                        p.PersonId = (await App.Instance.CreatePersonAsync(GroupName, p.Faces.Select(face => Guid.Parse(face.FaceId)).ToArray(), p.PersonName)).PersonId.ToString();

                    }
                    catch (Exception ex)
                    {

                    }
                    await Task.WhenAll(tasks);




                }

            }
            btnClick.IsEnabled = true;
        }

        private async void Identify_Click(object sender, RoutedEventArgs e)
        {
            btnClick.IsEnabled = false;
            string photolocation;
            var watch = Stopwatch.StartNew();

            // Show file picker
            //  Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            //  dlg.DefaultExt = ".jpg";
            //  dlg.Filter = "Image files(*.jpg) | *.jpg";
            //  var result = dlg.ShowDialog();
            try
            {


                // Using Windows.Media.Capture.CameraCaptureUI API to capture a photo



                var devices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);
                if (devices.Count == 0)
                {
                    MessageDialog dlg2 = new MessageDialog("Unable to connect to a video capture device");
                    await dlg2.ShowAsync();

                    return;
                }

                //try to connect to webcam
                var mediaCapture = new MediaCapture();
                await mediaCapture.InitializeAsync();


                var jpgProperties = ImageEncodingProperties.CreateJpeg();
                jpgProperties.Width = 800;
                jpgProperties.Height = 600;
                var properties = jpgProperties.Properties;

                Guid photoID = System.Guid.NewGuid();
                photolocation = photoID.ToString() + ".jpg";  //file name
                using (var randomAccessStream = new InMemoryRandomAccessStream())
                {
                    await mediaCapture.CapturePhotoToStreamAsync(jpgProperties, randomAccessStream);


                    randomAccessStream.Seek(0);
                    using (var ioStream = randomAccessStream.CloneStream())
                    {

                        WriteableBitmap bitmapimg = new WriteableBitmap(800, 600);
                        var pixelbuffer = bitmapimg.PixelBuffer;
                        bitmapimg.SetSource(ioStream);
                        ImageDisplay.Source = bitmapimg;

                        var folder = Windows.Storage.ApplicationData.Current.LocalFolder;

                        StorageFile file = await folder.CreateFileAsync(photolocation);


                        using (IRandomAccessStream fileStream = await file.OpenAsync(FileAccessMode.ReadWrite))
                        {




                            //dreamtcs to fix
                            BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, fileStream);
                            encoder.SetPixelData(BitmapPixelFormat.Rgba8, BitmapAlphaMode.Straight, 800, 600, 96, 96, pixelbuffer.ToArray());
                            await encoder.FlushAsync();

                        }

                    }
                }

                //appSettings[photoKey] = file.Path;

                // User picked one image
                // Clear previous detection and identification results
                TargetFaces.Clear();
                SelectedFile = photolocation;

                var sw = Stopwatch.StartNew();
                //dreamtcs to fix
                var imageInfo = await UIHelper.GetImageInfoForRendering(photolocation);

                // Call detection REST API
                var folder2 = Windows.Storage.ApplicationData.Current.LocalFolder;



                StorageFile file2 = await folder2.GetFileAsync(photolocation);
                using (var fileStream = await file2.OpenAsync(FileAccessMode.Read))
                {
                    try
                    {
                        WriteableBitmap wbt = new WriteableBitmap(800, 600);

                        wbt.SetSource(fileStream.CloneStream());

                        var faces = await App.Instance.DetectAsync(fileStream.AsStream());

                        // Convert detection result into UI binding object for rendering
                        foreach (var face in UIHelper.CalculateFaceRectangleForRendering(faces, MaxImageSize, imageInfo))
                        {
                            TargetFaces.Add(face);
                        }

                        Output = Output.AppendLine(string.Format("Request: Identifying {0} face(s) in group \"{1}\"", faces.Length, GroupName));

                        // Identify each face
                        // Call identify REST API, the result contains identified person information
                        var identifyResult = await App.Instance.IdentifyAsync(GroupName, faces.Select(ff => ff.FaceId).ToArray());
                        for (int idx = 0; idx < faces.Length; idx++)
                        {
                            // Update identification result for rendering
                            var face = TargetFaces[idx];
                            var res = identifyResult[idx];
                            if (res.Candidates.Length > 0 && Persons.Any(p => p.PersonId == res.Candidates[0].PersonId.ToString()))
                            {
                                face.PersonName = Persons.Where(p => p.PersonId == res.Candidates[0].PersonId.ToString()).First().PersonName;
                            }
                            else
                            {
                                face.PersonName = "Unknown";
                            }
                            //int pen_thickness = 5;

                            //Rectangle rect = new Rectangle();
                            //rect.Width = face.Width;
                            //rect.Height = face.Height;
                            //Canvas.SetTop(rect, face.Top);
                            //Canvas.SetLeft(rect,face.Left);
                            //rect.Fill = new SolidColorBrush(Color.FromArgb(100,255,0,0));
                            //rect.Opacity = 100;

                            //MyCanvas.Children.Add(rect);

                            //chun siong do something here to draw rectangle
                        }

                        watch.Stop();
                        var elapsedMs = watch.ElapsedMilliseconds;
                        var outString = new StringBuilder();
                        ObservableCollection<string> emailList = new ObservableCollection<string>();

                        foreach (var face in TargetFaces)
                        {

                            outString.AppendFormat("Face {0} is identified as {1}. within " + elapsedMs + "ms", face.FaceId, face.PersonName);
                            //dreamtcs to accumulate email address and send.
                            if (!face.PersonName.ToLower().Equals("unknown"))
                            {
                                emailList.Add(face.PersonName + "@microsoft.com");
                            }
                        }
                        //send email
                        listBox.Items.Clear();
                       foreach (string x in emailList) {
                            listBox.Items.Add(x);
                        }
                        //await _mailHelper.ComposeAndSendMailAsync("MailSubject", "displayname", "cstan@microsoft.com");
                        Output = Output.AppendLine(string.Format("Response: Success. {0}", outString));
                    }
                    catch (ClientException ex)
                    {
                        Output = Output.AppendLine(string.Format("Response: {0}. {1}", ex.Error.Code, ex.Error.Message));
                    }
                }


            }
            catch (Exception ex)
            {
                ex.ToString();
            }

            btnClick.IsEnabled = true;
        }
        public async Task SignInCurrentUserAsync()
        {

            var outlookClient = await AuthenticationHelper.GetOutlookClientAsync("Mail");

            if (outlookClient != null)
            {
                _displayName = (string)_settings.Values["LoggedInUser"];
                _mailAddress = (string)_settings.Values["LoggedInUserEmail"];
                UserName.Text = _displayName;
            }

        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);


        }

        private async void loginbtn_Click(object sender, RoutedEventArgs e)
        {
            if (!_userLoggedIn)
            {

                await SignInCurrentUserAsync();
                if (!String.IsNullOrEmpty(_displayName))
                {
                    _userLoggedIn = true;
                }

            }
            else
            {
                _userLoggedIn = false;
                this._displayName = null;
                this._mailAddress = null;
            }

        }
    }
}
