using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SantaClause
{
    internal class NorthPole
    {
        private readonly SemaphoreSlim _hangar = new SemaphoreSlim(0, 8);
        private readonly SemaphoreSlim _lastReindeerInHangar = new SemaphoreSlim(0, 1);
        private readonly SemaphoreSlim _SantaWorkshop = new SemaphoreSlim(0, 1);
        private readonly SemaphoreSlim _panicRoom = new SemaphoreSlim(0, 3);
        private readonly Random _random = new Random();

        private object _hangarLock = new object();
        private object _panicRoomLock = new object();

        public int ReindeersInHangar { get; private set; } = 0;
        public int ElfsInPanicRoom { get; private set; } = 0;

        public async Task StartNorthPoleAsync()
        {
            new Thread(async () => { await SantaDoWorkAsync(); }).Start();

            for (int i = 0; i < 9; i++)
            {
                new Thread(async () => await ReindeerDoWorkAsync(i)).Start();
            }

            for (int i = 0; i < 10; i++)
            {
                new Thread(async () => await ElfDoWorkAsync()).Start();
            }
        }

        public async Task SantaDoWorkAsync()
        {
            while (true)
            {
                await _SantaWorkshop.WaitAsync();

                lock (_hangarLock)
                {
                    if (ReindeersInHangar == 9)
                    {
                        ReindeersInHangar = 0;

                        Console.WriteLine($"Release reindeers for delivering");
                        for (int i = 0; i < 8; i++)
                        {
                            _hangar.Release();
                        }

                        Console.WriteLine($"Santa in Going to handle presentes");
                        Thread.Sleep(2000);

                        Console.WriteLine($"Releasing last reindeer");
                        _lastReindeerInHangar.Release();
                    }
                }

                lock (_panicRoomLock)
                {
                    if (ElfsInPanicRoom == 3)
                    {
                        Console.WriteLine($"Handle full panic room");
                        Thread.Sleep(1000);

                        ElfsInPanicRoom = 0;

                        Console.WriteLine($"Release panic room");
                        for (int i = 0; i < 3; i++)
                        {
                            _panicRoom.Release();
                        }
                    }
                }
            }
        }

        public async Task ReindeerDoWorkAsync(int i)
        {
            while (true)
            {
                // Start with vacation
                //Console.WriteLine($"Reindeer {i} in thread {Thread.CurrentThread.ManagedThreadId} is on vacation");
                Thread.Sleep(_random.Next(5000, 8000));
                Console.WriteLine($"Reindeer {i} is going to hangar");
                var lastReindeer = false;

                lock (_hangarLock)
                {
                    ReindeersInHangar++;
                    if (ReindeersInHangar == 9)
                    {
                        Console.WriteLine($"Reindeers releasing Santa.");
                        lastReindeer = true;
                        _SantaWorkshop.Release();
                    }
                }

                // Waiting in hangar room
                if (lastReindeer)
                {
                    await _lastReindeerInHangar.WaitAsync();
                }
                else
                {
                    await _hangar.WaitAsync();
                }

                Console.WriteLine($"Reindeer delivering toys.");
                Thread.Sleep(_random.Next(3000, 6000));
                Console.WriteLine($"Reindeer back on vacation.");
            }
        }

        public async Task ElfDoWorkAsync()
        {
            while (true)
            {
                Console.WriteLine("Elf is doing work.");
                Thread.Sleep(2000);
                if (_random.Next(0, 100) < 11)
                {
                    // 10 % chance to panic
                    Console.WriteLine($"Elf is going to panic room.");

                    lock (_panicRoomLock)
                    {
                        if (ElfsInPanicRoom == 3)
                        {
                            continue;
                        }

                        ElfsInPanicRoom++;

                        if (ElfsInPanicRoom == 3)
                        {
                            Console.WriteLine($"Elfs releasing Santa.");
                            _SantaWorkshop.Release();
                        }
                    }

                    await _panicRoom.WaitAsync();
                }
            }
        }
    }
}
