// See https://aka.ms/new-console-template for more information

using System.Runtime.CompilerServices;

class Program
{
    static async Task Main()
    {
        Console.ReadKey();
        var task = await Task.WhenAny(new []
        {
            Task.Delay(2000).ContinueWith(_ => Task.FromResult(2000)),
            Task.Delay(2500).ContinueWith(_ => Task.FromResult(2500)),
            Task.Delay(3000).ContinueWith(_ => Task.FromResult(3000))
        });
        
        Console.WriteLine("Task.WhenAny: " + task.Result.Result);
        
        var task1 = await WhenAny1(new []
        {
            Task.Delay(2000).ContinueWith(_ => Task.FromResult(2000)),
            Task.Delay(2500).ContinueWith(_ => Task.FromResult(2500)),
            Task.Delay(3000).ContinueWith(_ => Task.FromResult(3000))
        });
        
        Console.WriteLine("My WhenAny1: " + task1.Result);
        
        var task2 = await WhenAny2(new []
        {
            Task.Delay(2000).ContinueWith(_ => Task.FromResult(2000)),
            Task.Delay(2500).ContinueWith(_ => Task.FromResult(2500)),
            Task.Delay(3000).ContinueWith(_ => Task.FromResult(3000))
        });
        
        Console.WriteLine("My WhenAny2: " + task2.Result.Result);

        var task3 = await WhenAny3(new []
        {
            Task.Delay(2000).ContinueWith(_ => Task.FromResult(2000)),
            Task.Delay(2500).ContinueWith(_ => Task.FromResult(2500)),
            Task.Delay(3000).ContinueWith(_ => Task.FromResult(3000))
        });
        
        Console.WriteLine("My WhenAny3: " + task3.Result.Result);
    }
    
    //Реализация через объект синхронизации доступа к разделяемым ресурсам уровня ядра операционной системы
    static Task<T> WhenAny1<T>(Task<T>[] tasks)
    {
        //Вызываем статический метод на абстрактном классе который ожидает пока завершится любой 
        // из заблокированных потоков помещеный внутрь метода WaitAny
        // и получаем индекс первой завершившейся задачи
        int first = WaitHandle.WaitAny(tasks
                //Делаем upcast экземпляра класса Task до базового типа IAsyncResult
                //Добавляем в коллекцию экземпляр класса ManualResetEvent из свойства AsyncWaitHandle 
                .Select(s => (s as IAsyncResult).AsyncWaitHandle)
                .ToArray());
        
        return tasks[first]; //Возвращаем задачу по индексу
    } 

    static Task<Task<T>> WhenAny2<T>(Task<T>[] tasks)
    {
        //Создаем TaskCompletionSource
        TaskCompletionSource<Task<T>> source = new TaskCompletionSource<Task<T>>();
        foreach (var item in tasks)
        {
            //Задаем задаче из масива, заверешение в виде установки результата выполнения задачи
            // в метод SetResult.
            item.ContinueWith(s => source.SetResult(s));
        }
        return source.Task; // Возвращаем задачу созданую TaskCompletionSource и содержащую первую выполеную задачу
    }
    
    static Task<Task<T>> WhenAny3<T>(Task<T>[] tasks)
    {
        //Тупо ожидаем какая задача завершится первой првоеряя свойство 
        //IsCompleted на каждой задаче в массиве
        while (true)
        {
            foreach (var item in tasks)
            {
                if (item.IsCompleted)
                {
                    return Task<Task<T>>.FromResult(item);
                }
            }
        }
    }
}

