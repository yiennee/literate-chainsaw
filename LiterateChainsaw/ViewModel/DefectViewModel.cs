using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;
using ZephyrInnovations;

namespace LiterateChainsaw.ViewModel
{
    public class DefectViewModel : VMBase
    {
        private ObservableCollection<Classification> _defectList = new ObservableCollection<Classification>();
        public ObservableCollection<Classification> DefectList
        {
            get => _defectList;
            set { _defectList = value; OnPropertyChanged(nameof(DefectList)); }
        }

        private int _totalUnitCount;
        public int TotalUnitCount
        {
            get => _totalUnitCount;
            set { _totalUnitCount = value; OnPropertyChanged(nameof(TotalUnitCount)); }
        }

        public DefectViewModel()
        {
            //TotalUnitCount = 1;
            _defectList.CollectionChanged += (sender, e) =>
            {
                //if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add ||
                //    e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
                //{
                //    UpdatePercentage(this, new PropertyChangedEventArgs(""));
                //}

                if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                {
                    foreach (Classification addeditem in e.NewItems)
                    {
                        addeditem.CountChanged += UpdateCountPercentage;
                    }
                }

                UpdateCountPercentage(this, new EventArgs());
            };



            //DefectList.Add(new Classification(UpdatePercentage));
        }
        public void UpdateCountPercentage(object sender, EventArgs e)
        {
            // Calculate the sum of scores
            var totalCount = _defectList.Sum(p => p.Count);
            Console.Write($"TotalFailUnit={totalCount}");

            //replace totalCount with CurrentInspectedCount

            foreach (var defect in _defectList)
            {
                defect.Perc = Convert.ToDouble(defect.Count) / _totalUnitCount * 100f;

                Console.Write($"{defect.defectCodeData.DEFECT_CODE}={defect.Perc}%");
            }


            //var AllDefectCount = ClassificationViewModel.DefectList.Select(q => q.defectCount).Sum();

            //foreach (var item in ClassificationViewModel.DefectList)
            //{
            //    item.defectPerc = Convert.ToDouble(item.defectCount / AllDefectCount);
            //}

            // Notify subscribers that the properties have changed
            OnPropertyChanged(nameof(DefectList));
        }

        public void UpdatePercentage(object sender, PropertyChangedEventArgs e)
        {
            // Calculate the sum of scores
            var totalCount = _defectList.Sum(p => p.Count);
            Console.Write($"TotalInspectedUnit={totalCount}");

            foreach (var defect in _defectList)
            {
                defect.Perc = Convert.ToDouble(defect.Count) / totalCount * 100f;

                Console.Write($"{defect.defectCodeData.DEFECT_CODE}={defect.Perc}%");
            }


            //var AllDefectCount = ClassificationViewModel.DefectList.Select(q => q.defectCount).Sum();

            //foreach (var item in ClassificationViewModel.DefectList)
            //{
            //    item.defectPerc = Convert.ToDouble(item.defectCount / AllDefectCount);
            //}

            // Notify subscribers that the properties have changed
            OnPropertyChanged(nameof(DefectList));
        }
    }
    public class Classification : VMBase
    {
        private DefectCodeData _defectCodeData;
        private int _count;
        private int _VProContributes;
        private int _VidiContributes;
        private double _perc;

        public DefectCodeData defectCodeData
        {
            get => _defectCodeData;
            set { _defectCodeData = value; OnPropertyChanged(nameof(defectCodeData)); }
        }

        public int Count
        {
            get => _count;
            set { _count = value; OnCountChanged(); OnPropertyChanged(nameof(Count)); }
        }

        public int VProContributes
        {
            get => _VProContributes;
            set { _VProContributes = value; OnPropertyChanged(nameof(VProContributes)); }
        }

        public int VidiContributes
        {
            get => _VidiContributes;
            set { _VidiContributes = value; OnPropertyChanged(nameof(VidiContributes)); }
        }

        public double Perc
        {
            get => _perc;
            set { _perc = value; OnPropertyChanged(nameof(Perc)); }
        }

        public Action UpdateStatistics { get; }

        //public string Color
        //{
        //    get => Color;
        //    set { _color = value; }//} OnPropertyChanged(nameof(defectcolor)); }
        //}
        public Classification()
        {

        }
        public Classification(Action UpdateStatistics) => this.UpdateStatistics = UpdateStatistics;

        public event EventHandler CountChanged;
        protected virtual void OnCountChanged()
        {
            CountChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
