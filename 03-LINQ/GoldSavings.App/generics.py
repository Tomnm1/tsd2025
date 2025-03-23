import random
from typing import Generic, TypeVar, List

T = TypeVar('T')

class RandomList(Generic[T]):
    def __init__(self):
        self.items: List[T] = []

    def add(self, element: T):
        if random.choice([True, False]):
            self.items.insert(0, element)
        else:
            self.items.append(element)

    def get(self, max_index: int) -> T:
        if self.is_empty():
            raise IndexError("List is empty.")
        actual_max = min(max_index, len(self.items) - 1)
        return random.choice(self.items[:actual_max + 1])

    def is_empty(self) -> bool:
        return len(self.items) == 0



rl = RandomList[int]()
rl.add(10)
rl.add(20)
rl.add(30)

print(f"List empty? {rl.is_empty()}")

random_element = rl.get(2)
print(f"Random element {random_element}")


gold_prices = [
    {'date': '2024-03-01', 'price': 262.10},
    {'date': '2024-03-04', 'price': 263.09},
    {'date': '2024-03-05', 'price': 268.59},
    {'date': '2024-03-06', 'price': 273.37},
    {'date': '2024-03-07', 'price': 273.01},
    {'date': '2024-03-08', 'price': 273.35},
    {'date': '2024-03-11', 'price': 275.00}
]

top3_highest = sorted(gold_prices, key=lambda x: x['price'], reverse=True)[:3]

print("Top 3 highest gold prices:")
for gp in top3_highest:
    print(f"Date: {gp['date']}, Price: {gp['price']}")