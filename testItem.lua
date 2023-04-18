function SetDefault_testItem()
	item.damage = 50
	item.width = 40
	item.height = 40
	item.useTime = 20
	item.useAnimation = 20
	item.useStyle = 1
	item.knockBack = 6
	item.value = 10000
	item.rare = 2
	item.autoReuse = true
end
override function UpdateInventory_testItem()
        NewText("hello override!")
end
