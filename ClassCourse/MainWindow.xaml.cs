using CsvHelper;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace ClassCourse
{
    public partial class MainWindow : Window
    {
        List<Course> courses = new List<Course>();
        List<Teacher> teachers = new List<Teacher>();
        List<Student> students = new List<Student>();
        List<Record> studentRegistration = new List<Record>();
        Teacher selected_Teacher;
        Course selected_Course;
        Student selected_Student;
        Record selected_Record;
        public MainWindow()
        {
            InitializeComponent();
            lvRegistration.Items.Clear();
            ls_Teachers.SelectionChanged += Ls_Teachers_SelectionChanged;//委派
            tv_Teachers.SelectedItemChanged += Tv_Teachers_SelectedItemChanged;
            lvRegistration.SelectionChanged += LvRegistration_SelectionChanged;

        }
        public void Subject_Click(object sender, RoutedEventArgs e)
        {
            courses.Clear();
            teachers.Clear();
            studentRegistration.Clear();
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "讀取";
            openFileDialog.DefaultExt = ".csv";
            openFileDialog.Filter = "CSV file (*.csv)|*.csv| All file (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                string path = openFileDialog.FileName;
                using (var reader = new StreamReader(path, Encoding.Default))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    csv.Read();
                    csv.ReadHeader();
                    while (csv.Read())
                    {
                        teachers.Add(new Teacher() { TeacherName = csv.GetField<string>(0) });
                    }
                }
                for (int i = 0; i < teachers.Count; i++)
                {
                    for (int j = teachers.Count - 1; j > i; j--)
                    {
                        if (teachers[i].TeacherName == teachers[j].TeacherName)
                        {
                            teachers.RemoveAt(j);
                        }
                    }
                }
                using (var reader = new StreamReader(path, Encoding.Default))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    csv.Read();
                    csv.ReadHeader();
                    while (csv.Read())
                    {
                        for (int i = 0; i < teachers.Count; i++)
                        {
                            if (teachers[i].TeacherName == csv.GetField<string>(0))
                            {
                                teachers[i].Courses.Add(new Course(teachers[i])
                                {
                                    CourseName = csv.GetField<string>(1),//欄位
                                    Point = csv.GetField<int>(2),
                                    Type = csv.GetField<string>(3),
                                    OpeningClass = csv.GetField<string>(4),
                                    ClassTime = csv.GetField<string>(5)
                                });

                                courses.Add(new Course(teachers[i])
                                {
                                    CourseName = csv.GetField<string>(1),
                                    Point = csv.GetField<int>(2),
                                    Type = csv.GetField<string>(3),
                                    OpeningClass = csv.GetField<string>(4),
                                    ClassTime = csv.GetField<string>(5)
                                });
                            }
                        }
                    }
                }
            }
            tv_Teachers.ItemsSource = teachers;
            tv_Teachers.Items.Refresh();
            ls_Teachers.ItemsSource = courses;
            ls_Teachers.Items.Refresh();
            lvRegistration.Items.Refresh();
        }//科目資料開讀檔

        private void Student_Click(object sender, RoutedEventArgs e)
        {
            students.Clear();
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "讀取";
            openFileDialog.DefaultExt = ".csv";
            openFileDialog.Filter = "CSV file (*.csv)|*.csv| All file (*.*)|*.*";
            try
            {
                if (openFileDialog.ShowDialog() == true)
                {
                    string path = openFileDialog.FileName;
                    using (var reader = new StreamReader(path, Encoding.Default))
                    using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                    {
                        csv.Read();
                        csv.ReadHeader();
                        csv.Configuration.HasHeaderRecord = false;
                        students = csv.GetRecords<Student>().ToList();
                    }
                    ComboBox_1.ItemsSource = students;
                }
            }
            catch
            {
                StatusLabel.Content = "開檔失敗";
            }
        }//學生名單開讀檔
        private void registrationButton_Click(object sender, RoutedEventArgs e)//把選擇的加到右邊
        {
            try
            {
                selected_Student = ComboBox_1.SelectedItem as Student;
                if (tv_Teachers.SelectedItem is Course)
                {
                    selected_Course = tv_Teachers.SelectedItem as Course;
                }
                else
                    selected_Course = ls_Teachers.SelectedItem as Course;
                selected_Teacher = selected_Course.Tutor;

                Student selectedStudent = new Student()
                {
                    StudentID = selected_Student.StudentID,
                    StudentName = selected_Student.StudentName
                };

                Record currentRecord = new Record
                {
                    SelectedStudent = selectedStudent,
                    TeacherName = selected_Teacher.TeacherName,
                    SelectedCourse = selected_Course
                };

                selected_Teacher.Courses.Remove(selected_Course); //移除
                courses.Remove(selected_Course);
                studentRegistration.Add(currentRecord);
                lvRegistration.ItemsSource = studentRegistration;

                for (int i = 0; i < courses.Count; i++)
                {
                    if (courses[i].ToString() == selected_Course.ToString())
                    {
                        courses.RemoveAt(i);
                    }
                }

                for (int i = 0; i < teachers.Count; i++)
                {
                    for (int j = 0; j < teachers[i].Courses.Count; j++)
                    {
                        if (teachers[i].Courses[j].ToString() == selected_Course.ToString())
                        {
                            teachers[i].Courses.RemoveAt(j);
                        }
                    }
                }

                lvRegistration.Items.Refresh(); //刷新
                tv_Teachers.Items.Refresh();
                ls_Teachers.Items.Refresh();

            }

            catch
            {
                StatusLabel.Content = "請選取學生和課程";
            }
        }

        private void retakeButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                selected_Record = lvRegistration.SelectedItem as Record;
                selected_Student = selected_Record.SelectedStudent;
                selected_Course = selected_Record.SelectedCourse;
                selected_Teacher = selected_Course.Tutor;
                int i, j;
                for (i = 0; i < teachers.Count; i++)
                {
                    if (teachers[i].TeacherName == selected_Teacher.TeacherName)
                    {
                        teachers[i].Courses.Add(new Course(teachers[i])
                        {
                            CourseName = selected_Course.CourseName,
                            Point = selected_Course.Point,
                            Type = selected_Course.Type,
                            OpeningClass = selected_Course.OpeningClass,
                            ClassTime = selected_Course.ClassTime
                        });

                        break;
                    }
                }

                for (j = 0; j < courses.Count; j++)
                {
                    if (courses[j].Tutor.TeacherName == selected_Teacher.TeacherName)
                    {
                        courses.Insert(j, new Course(teachers[i])
                        {
                            CourseName = selected_Course.CourseName,
                            Point = selected_Course.Point,
                            Type = selected_Course.Type,
                            OpeningClass = selected_Course.OpeningClass,
                            ClassTime = selected_Course.ClassTime
                        });

                        break;
                    }
                    else
                    {
                        courses.Add(new Course(teachers[i])
                        {
                            CourseName = selected_Course.CourseName,
                            Point = selected_Course.Point,
                            Type = selected_Course.Type,
                            OpeningClass = selected_Course.OpeningClass,
                            ClassTime = selected_Course.ClassTime
                        });

                        break;
                    }
                }
                studentRegistration.Remove(selected_Record);//刪除
                lvRegistration.Items.Refresh();
                tv_Teachers.Items.Refresh();
                ls_Teachers.Items.Refresh();//更新
            }
            catch
            {
                StatusLabel.Content = "請選取移除課程";
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var SaveFileDialog = new SaveFileDialog();
            SaveFileDialog.Title = "存檔";
            SaveFileDialog.DefaultExt = ".txt";
            SaveFileDialog.Filter = "csv file (*.csv*)|*.csv*";
            SaveFileDialog.FileName = "4a8g0044.csv";
            if (lvRegistration.ItemsSource != null)
            {
                if (SaveFileDialog.ShowDialog() == true)
                {
                    string path = SaveFileDialog.FileName;
                    //var currenTime = DateTime.Now;
                    if (File.Exists(path) == false)
                    {
                        string text = $"學號,姓名,課程名稱,指導老師,學分數,必選修,開課班級,上課時間\n";
                        File.AppendAllText(path, text, Encoding.Default);
                    }
                    foreach (Record r in studentRegistration)
                    {
                        string text = $"{r.SelectedStudent.StudentID},{r.SelectedStudent.StudentName},{r.SelectedCourse.CourseName},{r.TeacherName},{r.SelectedCourse.Point},{r.SelectedCourse.Type},{r.SelectedCourse.OpeningClass},{r.SelectedCourse.ClassTime}\n";
                        File.AppendAllText(path, text, Encoding.Default);
                    }
                }
            }
        }
        private void Ls_Teachers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ls_Teachers.SelectedItem is Course)
            {
                selected_Course = ls_Teachers.SelectedItem as Course;
                selected_Teacher = selected_Course.Tutor;
                StatusLabel.Content = selected_Teacher.ToString() + selected_Course.ToString();
            }
        }

        private void Tv_Teachers_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (tv_Teachers.SelectedItem is Course)
            {
                selected_Course = tv_Teachers.SelectedItem as Course;
                selected_Teacher = selected_Course.Tutor;
                StatusLabel.Content = selected_Teacher.ToString() + selected_Course.ToString();
            }
        }

        private void LvRegistration_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lvRegistration.SelectedItem is Record)
            {
                selected_Record = lvRegistration.SelectedItem as Record;
                selected_Course = selected_Record.SelectedCourse;
                selected_Student = selected_Record.SelectedStudent;
                selected_Teacher = selected_Record.SelectedCourse.Tutor;
                StatusLabel.Content = selected_Student.ToString() + " " + selected_Course.ToString() + " " + selected_Teacher.ToString();
            }
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void lvRegistration_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {

        }
    }
    public class Course//課程
    {
        public string CourseName { set; get; }
        public int Point { set; get; }
        public string Type { set; get; }
        public string OpeningClass { set; get; }
        public string ClassTime { set; get; }
        public Teacher Tutor { set; get; }
        public Course(Teacher tutor)
        {
            this.Tutor = tutor;
        }
        public override string ToString()
        {
            return $"{CourseName} {Type} {Point}學分 開課班級:{OpeningClass}";
        }
    }
    public class Teacher
    {
        public string TeacherName { set; get; }
        public ObservableCollection<Course> Courses { set; get; }
        public Teacher()
        {
            this.Courses = new ObservableCollection<Course>();
        }
        public override string ToString()
        {
            return $"教師姓名:{TeacherName} ";
        }
    }//老師

    public class Student
    {
        public string StudentID { set; get; }
        public string StudentName { set; get; }

        public override string ToString()
        {
            return StudentID.ToString() + " " + StudentName.ToString();
        }
    }//學生

    public class Record//每一筆過去的資料
    {
        public Student SelectedStudent { set; get; }
        public string TeacherName { set; get; }
        public Course SelectedCourse { set; get; }

        public ObservableCollection<Teacher> R_teacher { set; get; }
        public Record()
        {
            this.R_teacher = new ObservableCollection<Teacher>();
        }
    }
}