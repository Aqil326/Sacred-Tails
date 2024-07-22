using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Timba.SacredTails.UiHelpers
{
    public class CalendarController : MonoBehaviour
    {
        public TMP_Text target;
        public TMP_Text _yearNumText;
        public TMP_Text _monthNumText;

        public CalendarDateItem itemPrefab;

        public List<CalendarDateItem> dateItems = new List<CalendarDateItem>();
        const int _totalDateNum = 42;

        private DateTime dateTime;
        public UnityEvent onSelectDate;

        void Start()
        {
            Vector3 startPos = itemPrefab.transform.localPosition;
            dateItems.Clear();
            dateItems.Add(itemPrefab);

            for (int i = 1; i < _totalDateNum; i++)
            {
                CalendarDateItem item = Instantiate(itemPrefab);
                item.name = "Item" + (i + 1).ToString();
                item.transform.SetParent(itemPrefab.transform.parent);
                item.transform.localScale = Vector3.one;
                item.transform.localRotation = Quaternion.identity;
                item.transform.localPosition = new Vector3(i % 7 * 31 + startPos.x, startPos.y - i / 7 * 25, startPos.z);
                item.onClickDate += OnDateItemClick;

                dateItems.Add(item);
            }

            dateTime = DateTime.Now;

            CreateCalendar();
        }

        void CreateCalendar()
        {
            DateTime firstDay = dateTime.AddDays(-(dateTime.Day - 1));
            int index = GetDays(firstDay.DayOfWeek);

            int date = 0;
            for (int i = 0; i < _totalDateNum; i++)
            {
                TMP_Text label = dateItems[i].GetComponentInChildren<TMP_Text>();
                dateItems[i].gameObject.SetActive(false);

                if (i >= index)
                {
                    DateTime thatDay = firstDay.AddDays(date);
                    if (thatDay.Month == firstDay.Month)
                    {
                        dateItems[i].gameObject.SetActive(true);

                        label.text = (date + 1).ToString();
                        date++;
                    }
                }
            }
            _yearNumText.text = dateTime.Year.ToString();
            _monthNumText.text = dateTime.Month.ToString();
        }

        int GetDays(DayOfWeek day)
        {
            switch (day)
            {
                case DayOfWeek.Monday: return 1;
                case DayOfWeek.Tuesday: return 2;
                case DayOfWeek.Wednesday: return 3;
                case DayOfWeek.Thursday: return 4;
                case DayOfWeek.Friday: return 5;
                case DayOfWeek.Saturday: return 6;
                case DayOfWeek.Sunday: return 0;
            }

            return 0;
        }
        public void YearPrev()
        {
            dateTime = dateTime.AddYears(-1);
            CreateCalendar();
        }

        public void YearNext()
        {
            dateTime = dateTime.AddYears(1);
            CreateCalendar();
        }

        public void MonthPrev()
        {
            dateTime = dateTime.AddMonths(-1);
            CreateCalendar();
        }

        public void MonthNext()
        {
            dateTime = dateTime.AddMonths(1);
            CreateCalendar();
        }

        public void OnDateItemClick(string day)
        {
            dayText = day;
            target.text = $"Year: {_yearNumText.text}, Month: {_monthNumText.text} Day: {dayText}";
            onSelectDate?.Invoke();
        }

        private string dayText;
        public TMP_Dropdown hour;
        public TMP_Dropdown minute;
        public TMP_Dropdown second;

        public bool CalendarHasDate()
        {
            if (string.IsNullOrEmpty(dayText))
                return false;
            else
                return true;
        }

        public string CreateDateForTournament()
        {
            //2022-12-09T09:12:00.000Z
            return $"{_yearNumText.text}-{ParseDateNumberToCorrectFormat(_monthNumText.text)}-{ParseDateNumberToCorrectFormat(dayText)}T{hour.options[hour.value].text}:{minute.options[minute.value].text}:{minute.options[minute.value].text}.000Z";
        }

        public string ParseDateNumberToCorrectFormat(string number)
        {
            int dayNumber = int.Parse(number);
            string dayTextFinal;
            if (dayNumber < 10)
                dayTextFinal = $"0{dayNumber}";
            else
                dayTextFinal = dayNumber.ToString();
            return dayTextFinal;
        }
    }
}