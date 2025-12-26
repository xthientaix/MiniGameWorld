using System.Collections.Generic;
using UnityEngine;

public static class Helper
{
    public static List<T> PickRandom<T>(List<T> source, int count)
    {
        // Tạo bản copy để không làm thay đổi list gốc
        List<T> temp = new(source);

        // Fisher–Yates shuffle
        for (int i = 0; i < temp.Count; i++)
        {
            int rand = Random.Range(i, temp.Count);
            (temp[i], temp[rand]) = (temp[rand], temp[i]);
        }

        // Lấy số lượng phần tử cần thiết
        count = Mathf.Clamp(count, 0, temp.Count);
        return temp.GetRange(0, count);
    }

    public static T WeightedRandom<T>(List<T> values, List<float> weights)
    {
        if (values.Count != weights.Count)
        {
            Debug.LogError("Số lượng values và weights phải bằng nhau!");
            return default;
        }

        // Tính tổng trọng số
        float total = 0f;
        foreach (float w in weights)
        { total += w; }

        // Random từ 0 → total
        float rand = Random.Range(0f, total);

        // Tìm phần tử tương ứng
        float cumulative = 0f;
        for (int i = 0; i < values.Count; i++)
        {
            cumulative += weights[i];
            if (rand <= cumulative)
            { return values[i]; }
        }

        // trường hợp hiếm khi rand == total nhưng total thực tế lại sai số do float dẫn tới rand > total
        return values[^1];
    }

    public static void MoveChildren(Transform fromParent, Transform toParent, bool setInactive = false)
    {
        /// Di chuyển tất cả object con từ parent nguồn sang parent đích.
        /// Có thể chọn set inactive sau khi di chuyển.

        // Duyệt ngược để tránh bị nhảy cóc khi thay đổi parent
        for (int i = fromParent.childCount - 1; i >= 0; i--)
        {
            Transform child = fromParent.GetChild(i);
            child.SetParent(toParent);

            if (setInactive)
            {
                child.gameObject.SetActive(false);
            }
        }
    }

}