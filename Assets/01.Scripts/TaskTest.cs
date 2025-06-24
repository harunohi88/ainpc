using System.Collections;
using UnityEngine;
using System.Threading.Tasks;

public class TaskTest : MonoBehaviour
{
    // '비동기': '동기'의 반대말로 어떤 '작업'을 실행할 때 그 작업이 완료되지 않아도
    //           다음 코드를 실행하는 방식
    // 그 '작업'의 특징: 시간이 오래걸린다. (ex. 연산량이 많거나, IO 작업 등)


    private async void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Space key pressed!");
            // Task task1 = new Task(LongLoop);
            Task<long> task2 = new Task<long>(LongLoop2);
            task2.Start();
            // task2.Wait();
            // long result = task2.Result;
            // Debug.Log(result);
            long result = await task2;
            Debug.Log(result);
        }
    }
 

    // 연산량이 많은 작업
    private void LongLoop()
    {
        long sum = 1;
        for (long i = 1; i < 10000000000; ++i)
        {
            sum *= i;
        }
        
        Debug.Log("LongLoop finished!");
    }
    
    private long LongLoop2()
    {
        long sum = 1;
        for (long i = 1; i < 10000000000; ++i)
        {
            sum *= i;
        }
        
        Debug.Log("LongLoop finished!");

        return (sum);
    }
    
    private IEnumerator LongLoop_Coroutine()
    {
        for (long i = 1; i < 10000000000; ++i) // 
        {
            if (i % 100000 == 0)
            {
                yield return null;
            }
        }
        Debug.Log("LongLoop_Coroutine finished!");
    }
}