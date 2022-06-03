namespace Tyfyter.Utils {
	public static class MiscUtils {
        public static T[] BuildArray<T>(int length, params int[] nonNullIndeces) where T : new() {
            T[] o = new T[length];
            for(int i = 0; i < nonNullIndeces.Length; i++) {
                if(nonNullIndeces[i] == -1) {
                    for(i = 0; i < o.Length; i++) {
                        o[i] = new T();
                    }
                    break;
                }
                o[nonNullIndeces[i]] = new T();
            }
            return o;
        }
        public struct AccumulatingAverage {
            double sum;
            int count;
            public void Add(double value) {
                sum += value;
                count++;
            }
            public static AccumulatingAverage Add(AccumulatingAverage average, double value) {
                average.Add(value);
                return average;
            }
            public static explicit operator double(AccumulatingAverage value) {
                return value.sum / value.count;
            }
            public static explicit operator int(AccumulatingAverage value) {
                return (int)(value.sum / value.count);
            }
            public static explicit operator string(AccumulatingAverage value) {
                return $"{value.sum / value.count}";
            }
        }
	}
}