#### Description

- A stable crossplatform ViewPager for iOS/Android
- There is no memory leak
- Easy to use.

#### Setup
* Available on NuGet: https://www.nuget.org/packages/ViewPagerForms [![NuGet]
* Install in your PCL project and Client projects.

**XAML:**

First add the xmlns namespace:

```xml
xmlns:cv="clr-namespace:ViewPagerForms.Forms;assembly=ViewPagerForms.Forms"
```

Then add the xaml:

```xml
<cv:ViewPagerControl x:Name="viewpager" VerticalOptions="FillAndExpand" Position="{Binding Position, Mode=TwoWay}" ItemsSource="{Binding ListItems}" Infinite="false">
    <cv:ViewPagerControl.ItemTemplate>
        <DataTemplate>
            <StackLayout BackgroundColor="Olive" Padding="10">
                <StackLayout BackgroundColor="Red" VerticalOptions="FillAndExpand">
                    <Label Text="{Binding .}" FontSize="30" HorizontalOptions="CenterAndExpand" VerticalOptions="CenterAndExpand" TextColor="Yellow"/>
                </StackLayout>
            </StackLayout>
        </DataTemplate>
    </cv:ViewPagerControl.ItemTemplate>
</cv:ViewPagerControl>
```

**Code Behind:**
```CSharp
    public class ViewModel
    {
        public ObservableCollection<int> ListItems { get; set; }
        public int Position { get; set; }

        public ViewModel()
        {
            ListItems = new ObservableCollection<int>() { 1, 2, 3, 4, 5, 6, 9 };
            Position = 5;
        }
    }
```

(https://www.youtube.com/watch?v=Liftcv-N_zo)
