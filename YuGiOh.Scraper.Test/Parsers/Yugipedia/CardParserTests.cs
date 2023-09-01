namespace YuGiOh.Scraper.Test.Parsers.Yugipedia;

public class CardParserTests
{

    [Fact]
    public async Task GetId_ExpectNotZero()
    {

        var parser = new TestCardParser("346916", "Odd-Eyes Absolute Dragon");
        var actual = await parser.GetId();

        Assert.NotEqual(0, actual);

    }

    [Fact]
    public async Task GetName_ExpectNotNullOrWhiteSpace()
    {

        var parser = new TestCardParser("346916", "Odd-Eyes Absolute Dragon");
        var actual = await parser.GetName();

        Assert.False(string.IsNullOrWhiteSpace(actual));

    }

    [Fact]
    public async Task GetRealName_ExpectNotNullOrWhiteSpace()
    {

        var parser = new TestCardParser("696780", "Burning Soul (card)");
        var actual = await parser.GetRealName();

        Assert.False(string.IsNullOrWhiteSpace(actual));

    }

    [Fact]
    public async Task GetRealName_ExpectNullOrWhiteSpace()
    {

        var parser = new TestCardParser("714954", "Firewall Dragon Darkfluid - Neo Tempest Terahertz");
        var actual = await parser.GetRealName();

        Assert.True(string.IsNullOrWhiteSpace(actual));

    }

    [Fact]
    public async Task GetCardType_ExpectMonster()
    {

        //bruh, what is this name
        var parser = new TestCardParser("714954", "Firewall Dragon Darkfluid - Neo Tempest Terahertz");
        var actual = await parser.GetCardType();

        Assert.Equal("Monster", actual);

    }

    [Fact]
    public async Task GetProperty_ExpectNotNullOrWhiteSpace()
    {

        var parser = new TestCardParser("131469", "Vanity's Emptiness");
        var actual = await parser.GetProperty();

        Assert.False(string.IsNullOrWhiteSpace(actual));

    }

    [Fact]
    public async Task GetProperty_ExpectNullOrWhiteSpace()
    {

        var parser = new TestCardParser("714954", "Firewall Dragon Darkfluid - Neo Tempest Terahertz");
        var actual = await parser.GetProperty();

        Assert.True(string.IsNullOrWhiteSpace(actual));

    }

    [Fact]
    public async Task GetTypes_ExpectNotNullOrWhiteSpace()
    {

        var parser = new TestCardParser("714954", "Firewall Dragon Darkfluid - Neo Tempest Terahertz");
        var actual = await parser.GetTypes();

        Assert.False(string.IsNullOrWhiteSpace(actual));

    }

    [Fact]
    public async Task GetTypes_ExpectNullOrWhiteSpace()
    {

        var parser = new TestCardParser("526389", "Showdown of the Secret Sense Scroll Techniques");
        var actual = await parser.GetTypes();

        Assert.True(string.IsNullOrWhiteSpace(actual));

    }

    [Fact]
    public async Task GetAttribute_ExpectNotNullOrWhiteSpace()
    {

        var parser = new TestCardParser("714954", "Firewall Dragon Darkfluid - Neo Tempest Terahertz");
        var actual = await parser.GetAttribute();

        Assert.False(string.IsNullOrWhiteSpace(actual));

    }

    [Fact]
    public async Task GetAttribute_ExpectNullOrWhiteSpace()
    {

        var parser = new TestCardParser("526389", "Showdown of the Secret Sense Scroll Techniques");
        var actual = await parser.GetAttribute();

        Assert.True(string.IsNullOrWhiteSpace(actual));

    }

    [Fact]
    public async Task GetMaterials_ExpectNullOrWhiteSpace()
    {

        var parser = new TestCardParser("17371", "Mokey Mokey King");
        var actual = await parser.GetMaterials();

        Assert.True(string.IsNullOrWhiteSpace(actual));

    }

    // [Fact]
    // public async Task GetMaterials_ExpectNotNullOrWhiteSpace()
    // {
    //
    //     var parser = new TestCardParser("713092", "Arktos XII - Chronochasm Vaylantz");
    //     var actual = await parser.GetMaterials();
    //     
    //     Assert.False(string.IsNullOrWhiteSpace(actual));
    //
    // }

    [Fact]
    public async Task GetLore_ExpectNotNullOrWhiteSpace()
    {

        var parser = new TestCardParser("714954", "Firewall Dragon Darkfluid - Neo Tempest Terahertz");
        var actual = await parser.GetLore();

        Assert.False(string.IsNullOrWhiteSpace(actual));

    }

    [Fact]
    public async Task GetPendulumLore_IsPendulum_ExpectNotNullOrWhiteSpace()
    {

        var parser = new TestCardParser("406928", "Majespecter Fox - Kyubi");
        var actual = await parser.GetPendulumLore();

        Assert.False(string.IsNullOrWhiteSpace(actual));

    }

    [Fact]
    public async Task GetPendulumLore_IsNotPendulum_ExpectNullOrWhiteSpace()
    {

        var parser = new TestCardParser("714954", "Firewall Dragon Darkfluid - Neo Tempest Terahertz");
        var actual = await parser.GetPendulumLore();

        Assert.True(string.IsNullOrWhiteSpace(actual));

    }

    [Fact]
    public async Task GetTranslations_ExpectNotEmpty()
    {

        var parser = new TestCardParser("714954", "Firewall Dragon Darkfluid - Neo Tempest Terahertz");
        var actual = await parser.GetTranslations();

        Assert.NotEmpty(actual);

    }

    [Fact]
    public async Task GetTranslations_ExpectEmpty()
    {

        var parser = new TestCardParser("9987", "The Seal of Orichalcos (UDE promo)");
        var actual = await parser.GetTranslations();

        Assert.Empty(actual);

    }

    [Fact]
    public async Task GetArchetypes_ExpectNotEmpty()
    {

        var parser = new TestCardParser("346916", "Odd-Eyes Absolute Dragon");
        var actual = await parser.GetArchetypes();

        Assert.NotEmpty(actual);

    }

    [Fact]
    public async Task GetArchetypes_ExpectEmpty()
    {

        var parser = new TestCardParser("316084", "Card Advance");
        var actual = await parser.GetArchetypes();

        Assert.Empty(actual);

    }

    [Fact]
    public async Task GetSupports_ExpectNotEmpty()
    {

        var parser = new TestCardParser("405345", "Metaltron XII, the True Dracombatant");
        var actual = await parser.GetSupports();

        Assert.NotEmpty(actual);

    }

    [Fact]
    public async Task GetSupports_ExpectEmpty()
    {

        var parser = new TestCardParser("13042", "Guard Penalty");
        var actual = await parser.GetSupports();

        Assert.Empty(actual);

    }

    [Fact]
    public async Task GetAntiSupports_ExpectNotEmpty()
    {

        var parser = new TestCardParser("714733", "Duel Academy (card)");
        var actual = await parser.GetAntiSupports();

        Assert.NotEmpty(actual);

    }

    [Fact]
    public async Task GetAntiSupports_ExpectEmpty()
    {

        var parser = new TestCardParser("716383", "ReSolfachord Dreamia");
        var actual = await parser.GetAntiSupports();

        Assert.Empty(actual);

    }

    [Fact]
    public async Task GetLinkCount_ExpectLargerThanZero()
    {

        var parser = new TestCardParser("714954", "Firewall Dragon Darkfluid - Neo Tempest Terahertz");
        var actual = await parser.GetLinkCount();

        Assert.True(actual > 0);

    }

    [Fact]
    public async Task GetLinkCount_IsMonster_ExpectZero()
    {

        var parser = new TestCardParser("472491", "Imperion Magnum the Superconductive Battlebot");
        var actual = await parser.GetLinkCount();

        Assert.Equal(0, actual);

    }

    [Fact]
    public async Task GetLinkCount_IsNotMonster_ExpectZero()
    {

        var parser = new TestCardParser("526389", "Showdown of the Secret Sense Scroll Techniques");
        var actual = await parser.GetLinkCount();

        Assert.Equal(0, actual);

    }

    [Fact]
    public async Task GetLinkArrows_ExpectNotNullOrWhiteSpace()
    {

        var parser = new TestCardParser("714954", "Firewall Dragon Darkfluid - Neo Tempest Terahertz");
        var actual = await parser.GetLinkArrows();

        Assert.False(string.IsNullOrWhiteSpace(actual));

    }

    [Fact]
    public async Task GetLinkArrows_ExpectNull()
    {

        var parser = new TestCardParser("472491", "Imperion Magnum the Superconductive Battlebot");
        var actual = await parser.GetLinkArrows();

        Assert.Null(actual);

    }

    [Fact]
    public async Task GetAtk_ExpectNotNullOrWhiteSpace()
    {

        var parser = new TestCardParser("18849", "The Wicked Avatar");
        var actual = await parser.GetAtk();

        Assert.False(string.IsNullOrWhiteSpace(actual));

    }

    [Fact]
    public async Task GetAtk_ExpectNull()
    {

        var parser = new TestCardParser("526389", "Showdown of the Secret Sense Scroll Techniques");
        var actual = await parser.GetAtk();

        Assert.True(string.IsNullOrWhiteSpace(actual));

    }

    [Fact]
    public async Task GetDef_ExpectNotNullOrWhiteSpace()
    {

        var parser = new TestCardParser("18849", "The Wicked Avatar");
        var actual = await parser.GetDef();

        Assert.False(string.IsNullOrWhiteSpace(actual));

    }

    [Fact]
    public async Task GetDef_ExpectNullOrWhiteSpace()
    {

        var parser = new TestCardParser("526389", "Showdown of the Secret Sense Scroll Techniques");
        var actual = await parser.GetDef();

        Assert.True(string.IsNullOrWhiteSpace(actual));

    }

    [Fact]
    public async Task GetLevel_ExpectMoreThanZero()
    {

        var parser = new TestCardParser("472491", "Imperion Magnum the Superconductive Battlebot");
        var actual = await parser.GetLevel();

        Assert.True(actual > 0);

    }

    [Fact]
    public async Task GetLevel_IsXyz_ExpectNegativeOne()
    {

        var parser = new TestCardParser("306299", "Number 23: Lancelot, Dark Knight of the Underworld");
        var actual = await parser.GetLevel();

        Assert.Equal(-1, actual);

    }

    [Fact]
    public async Task GetLevel_IsLink_ExpectNegativeOne()
    {

        var parser = new TestCardParser("532966", "Cherubini, Ebon Angel of the Burning Abyss");
        var actual = await parser.GetLevel();

        Assert.Equal(-1, actual);

    }

    [Fact]
    public async Task GetLevel_ExpectNegativeOne()
    {

        var parser = new TestCardParser("526389", "Showdown of the Secret Sense Scroll Techniques");
        var actual = await parser.GetLevel();

        Assert.Equal(-1, actual);

    }

    [Fact]
    public async Task GetPendulumScale_ExpectLargerThanNegativeOne()
    {

        var parser = new TestCardParser("716922", "Superheavy Samurai Monk Warrior Big Benkei");
        var actual = await parser.GetPendulumScale();

        Assert.True(actual > -1);

    }

    [Fact]
    public async Task GetRank_ExpectNegativeOne()
    {

        var parser = new TestCardParser("716605", "Scareclaw Straddle");
        var actual = await parser.GetRank();

        Assert.Equal(-1, actual);

    }

    [Fact]
    public async Task GetRank_ExpectLargerThanNegativeOne()
    {

        var parser = new TestCardParser("239318", "Number 106: Giant Hand");
        var actual = await parser.GetRank();

        Assert.True(actual > -1);

    }

    [Fact]
    public async Task GetTcgExists_ExpectTrue()
    {

        var parser = new TestCardParser("502107", "Edge Imp Cotton Eater");
        var actual = await parser.GetTcgExists();

        Assert.True(actual);

    }

    [Fact]
    public async Task GetTcgExists_ExpectFalse()
    {

        var parser = new TestCardParser("5688", "Masahiro the Dark Clown");
        var actual = await parser.GetTcgExists();

        Assert.False(actual);

    }

    [Fact]
    public async Task GetOcgExists_ExpectTrue()
    {

        var parser = new TestCardParser("5717", "Prohibition");
        var actual = await parser.GetOcgExists();

        Assert.True(actual);

    }

    [Fact]
    public async Task GetOcgExists_ExpectFalse()
    {

        var parser = new TestCardParser("9987", "The Seal of Orichalcos (UDE promo)");
        var actual = await parser.GetOcgExists();

        Assert.False(actual);

    }

    [Fact]
    public async Task GetImgLink_ExpectNotNullOrWhiteSpace()
    {

        var parser = new TestCardParser("100449", "Stardust Xiaolong");
        var actual = await parser.GetImgLink();

        Assert.False(string.IsNullOrWhiteSpace(actual));

    }

    [Fact]
    public async Task GetUrl_ExpectNotNullOrWhiteSpace()
    {

        var parser = new TestCardParser("13834", "7");
        var actual = await parser.GetUrl();

        Assert.False(string.IsNullOrWhiteSpace(actual));

    }

    [Fact]
    public async Task GetPasscode_ExpectNotNullOrWhiteSpace()
    {

        var parser = new TestCardParser("631107", "Absorouter Dragon");
        var actual = await parser.GetPasscode();

        Assert.False(string.IsNullOrWhiteSpace(actual));

    }

    [Fact]
    public async Task GetOcgStatus_ExpectForbidden()
    {

        var parser = new TestCardParser("159", "Confiscation");
        var actual = await parser.GetOcgStatus();

        Assert.Equal("Forbidden", actual);

    }

    [Fact]
    public async Task GetOcgStatus_ExpectLimited()
    {

        var parser = new TestCardParser("278", "Exodia the Forbidden One");
        var actual = await parser.GetOcgStatus();

        Assert.Equal("Limited", actual);

    }

    [Fact]
    public async Task GetOcgStatus_ExpectSemiLimited()
    {

        var parser = new TestCardParser("14402", "Destiny HERO - Malicious");
        var actual = await parser.GetOcgStatus();

        Assert.Equal("Semi-Limited", actual);

    }

    [Fact]
    public async Task GetOcgStatus_ExpectUnlimited()
    {

        var parser = new TestCardParser("357417", "Sky Dragoons of Draconia");
        var actual = await parser.GetOcgStatus();

        Assert.Equal("Unlimited", actual);

    }

    [Fact]
    public async Task GetTcgStatus_ExpectForbidden()
    {

        var parser = new TestCardParser("159", "Confiscation");
        var actual = await parser.GetTcgAdvStatus();

        Assert.Equal("Forbidden", actual);

    }

    [Fact]
    public async Task GetTcgStatus_ExpectLimited()
    {

        var parser = new TestCardParser("278", "Exodia the Forbidden One");
        var actual = await parser.GetTcgAdvStatus();

        Assert.Equal("Limited", actual);

    }

    [Fact]
    public async Task GetTcgStatus_ExpectSemiLimited()
    {

        var parser = new TestCardParser("14402", "Destiny HERO - Malicious");
        var actual = await parser.GetTcgAdvStatus();

        Assert.Equal("Semi-Limited", actual);

    }

    [Fact]
    public async Task GetTcgStatus_ExpectedUnlimited()
    {

        var parser = new TestCardParser("357417", "Sky Dragoons of Draconia");
        var actual = await parser.GetTcgAdvStatus();

        Assert.Equal("Unlimited", actual);

    }

}